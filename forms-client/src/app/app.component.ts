import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormTemplate, FormTemplateService } from './form-template.service';
import { APP_TEXTS } from './app-texts';

@Component({ selector: 'app-root', standalone: true, imports: [CommonModule, ReactiveFormsModule], templateUrl: './app.component.html' })
export class AppComponent implements OnInit {
  readonly text = APP_TEXTS;
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(FormTemplateService);
  readonly form = this.fb.group({ name: ['', Validators.required], createdBy: ['hr-admin', Validators.required], fields: this.fb.array([]), approvalSteps: this.fb.array([]) });
  validationError = '';
  readonly templates$ = this.service.templates$;
  readonly loading$ = this.service.loading$;
  readonly error$ = this.service.error$;
  readonly saved$ = this.service.saved$;
  editingApprovalTemplate: number | null = null;
  approvalStatus = '';

  ngOnInit(): void { this.service.load().subscribe(); }

  get fields(): FormArray { return this.form.controls.fields; }
  get approvalSteps(): FormArray { return this.form.controls.approvalSteps; }
  addField(type: 'Text' | 'Date'): void { this.fields.push(this.fb.group({ label: [this.text.fieldTypeLabels[type], Validators.required], fieldType: [type], displayOrder: [this.fields.length + 1], isRequired: [false] })); }
  addStep(): void { this.approvalSteps.push(this.fb.group({ stepOrder: [this.approvalSteps.length + 1], name: ['', Validators.required], approverId: ['', Validators.required], actionType: [1] })); }
  remove(array: FormArray, index: number): void { array.removeAt(index); this.updateOrders(); }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.validationError = this.text.validationError;
      return;
    }
    this.validationError = '';
    this.updateOrders();
    this.service.create(this.form.getRawValue() as never).subscribe({
      next: () => this.resetForm()
    });
  }

  startApprovalSteps(template: FormTemplate): void {
    this.editingApprovalTemplate = template.id;
    this.approvalStatus = '';
    this.approvalSteps.clear();
    template.approvalSteps.forEach(step => this.approvalSteps.push(this.fb.group({
      stepOrder: [step.stepOrder],
      name: [step.name, Validators.required],
      approverId: [step.approverId, Validators.required],
      actionType: [step.actionType]
    })));
    this.addStep();
  }

  saveApprovalSteps(): void {
    if (this.editingApprovalTemplate === null || this.approvalSteps.invalid) {
      this.approvalSteps.markAllAsTouched();
      this.approvalStatus = this.text.approvalValidationError;
      return;
    }

    this.updateOrders();
    this.service.saveApprovalSteps(this.editingApprovalTemplate, this.approvalSteps.getRawValue() as never[]).subscribe({
      next: () => { this.approvalStatus = this.text.approvalSaved; this.editingApprovalTemplate = null; },
      error: () => this.approvalStatus = this.text.approvalSaveError
    });
  }

  dismissSavedMessage(): void { this.service.dismissSavedMessage(); }

  private resetForm(): void {
    this.form.reset({ name: '', createdBy: 'hr-admin' });
    this.fields.clear();
    this.approvalSteps.clear();
    this.validationError = '';
  }

  private updateOrders(): void {
    this.fields.controls.forEach((field, index) => field.patchValue({ displayOrder: index + 1 }));
    this.approvalSteps.controls.forEach((step, index) => step.patchValue({ stepOrder: index + 1 }));
  }
}
