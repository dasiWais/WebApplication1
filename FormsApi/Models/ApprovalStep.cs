namespace FormsApi.Models;

public sealed class ApprovalStep
{
    public int Id { get; set; }
    public int FormTemplateId { get; set; }
    public int StepOrder { get; set; }
    public required string Name { get; set; }
    public required string ApproverId { get; set; }
    public ApprovalActionType ActionType { get; set; }

    public FormTemplate FormTemplate { get; set; } = null!;
}

public enum ApprovalActionType
{
    Approve = 1,
    Reject = 2,
    ApproveOrReject = 3
}
