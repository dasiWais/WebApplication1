import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, finalize, shareReplay, tap, throwError } from 'rxjs';
import { APP_TEXTS } from './app-texts';

export interface FormField { label: string; fieldType: string; displayOrder: number; isRequired: boolean; }
export interface ApprovalStep { id?: number; stepOrder: number; name: string; approverId: string; actionType: number; }
export interface CreateFormTemplate { name: string; createdBy: string; fields: FormField[]; approvalSteps: ApprovalStep[]; }
export interface FormTemplate extends CreateFormTemplate { id: number; createdAtUtc: string; }

@Injectable({ providedIn: 'root' })
export class FormTemplateService {
  private readonly http = inject(HttpClient);
  private readonly url = 'http://localhost:5124/api/form-templates';
  private readonly templatesSubject = new BehaviorSubject<FormTemplate[]>([]);
  private readonly loadingSubject = new BehaviorSubject(false);
  private readonly errorSubject = new BehaviorSubject<string | null>(null);
  private readonly savedSubject = new BehaviorSubject(false);

  readonly templates$ = this.templatesSubject.asObservable();
  readonly loading$ = this.loadingSubject.asObservable();
  readonly error$ = this.errorSubject.asObservable();
  readonly saved$ = this.savedSubject.asObservable();

  dismissSavedMessage(): void { this.savedSubject.next(false); }

  load(): Observable<FormTemplate[]> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    this.savedSubject.next(false);
    return this.http.get<FormTemplate[]>(this.url).pipe(
      tap(templates => this.templatesSubject.next(templates)),
      catchError(error => {
        this.errorSubject.next(APP_TEXTS.loadError);
        return throwError(() => error);
      }),
      finalize(() => this.loadingSubject.next(false)),
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }

  create(request: CreateFormTemplate): Observable<FormTemplate> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    this.savedSubject.next(false);
    return this.http.post<FormTemplate>(this.url, request).pipe(
      tap(template => {
        this.templatesSubject.next([template, ...this.templatesSubject.value]);
        this.savedSubject.next(true);
      }),
      catchError(error => {
        this.errorSubject.next(APP_TEXTS.templateSaveError);
        return throwError(() => error);
      }),
      finalize(() => this.loadingSubject.next(false))
    );
  }

  saveApprovalSteps(templateId: number, approvalSteps: ApprovalStep[]): Observable<FormTemplate> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    return this.http.post<FormTemplate>(`${this.url}/${templateId}/approval-steps`, { approvalSteps }).pipe(
      tap(template => this.templatesSubject.next(this.templatesSubject.value.map(item => item.id === template.id ? template : item))),
      catchError(error => {
        this.errorSubject.next(APP_TEXTS.approvalSaveError);
        return throwError(() => error);
      }),
      finalize(() => this.loadingSubject.next(false))
    );
  }
}
