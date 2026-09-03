using FormsApi.Models;

namespace FormsApi.Contracts;

public sealed record CreateFormTemplateRequest(
    string Name,
    string CreatedBy,
    IReadOnlyList<CreateFormFieldRequest> Fields,
    IReadOnlyList<CreateApprovalStepRequest> ApprovalSteps);

public sealed record CreateFormFieldRequest(
    string Label,
    string FieldType,
    int DisplayOrder,
    bool IsRequired);

public sealed record CreateApprovalStepRequest(
    int StepOrder,
    string Name,
    string ApproverId,
    ApprovalActionType ActionType);

public sealed record AddApprovalStepsRequest(
    IReadOnlyList<CreateApprovalStepRequest> ApprovalSteps);

public sealed record FormTemplateResponse(
    int Id,
    string Name,
    DateTime CreatedAtUtc,
    string CreatedBy,
    IReadOnlyList<FormFieldResponse> Fields,
    IReadOnlyList<ApprovalStepResponse> ApprovalSteps);

public sealed record FormFieldResponse(
    int Id,
    string Label,
    string FieldType,
    int DisplayOrder,
    bool IsRequired);

public sealed record ApprovalStepResponse(
    int Id,
    int StepOrder,
    string Name,
    string ApproverId,
    ApprovalActionType ActionType);
