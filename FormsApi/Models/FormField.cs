namespace FormsApi.Models;

public sealed class FormField
{
    public int Id { get; set; }
    public int FormTemplateId { get; set; }
    public required string Label { get; set; }
    public FormFieldType FieldType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }

    public FormTemplate FormTemplate { get; set; } = null!;
}

public enum FormFieldType
{
    Text = 1,
    Date = 2,
    Number = 3,
    Select = 4,
    Checkbox = 5
}
