namespace FormsApi.Models;

public sealed class FormTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; }

    public List<FormField> Fields { get; set; } = [];
    public List<ApprovalStep> ApprovalSteps { get; set; } = [];
}
