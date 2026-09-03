using FormsApi.Contracts;
using FormsApi.Data;
using FormsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormsApi.Controllers;

[ApiController]
[Route("api/form-templates")]
public sealed class FormTemplatesController(FormsDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(FormTemplateResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FormTemplateResponse>> Create(
        CreateFormTemplateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            return BadRequest("Name and CreatedBy are required.");
        }

        var form = new FormTemplate
        {
            Name = request.Name.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            Fields = request.Fields.Select(field => new FormField
            {
                Label = field.Label.Trim(),
                FieldType = Enum.Parse<FormFieldType>(field.FieldType.Trim(), ignoreCase: true),
                DisplayOrder = field.DisplayOrder,
                IsRequired = field.IsRequired
            }).ToList(),
            ApprovalSteps = request.ApprovalSteps.Select(step => new ApprovalStep
            {
                Name = step.Name.Trim(),
                ApproverId = step.ApproverId.Trim(),
                StepOrder = step.StepOrder,
                ActionType = step.ActionType
            }).ToList()
        };

        db.FormTemplates.Add(form);
        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(form);
        return CreatedAtAction(nameof(GetById), new { id = form.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FormTemplateResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var forms = await db.FormTemplates
            .AsNoTracking()
            .OrderByDescending(form => form.CreatedAtUtc)
            .Select(form => ToResponseQuery(form))
            .ToListAsync(cancellationToken);

        return Ok(forms);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FormTemplateResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var form = await db.FormTemplates
            .AsNoTracking()
            .Include(template => template.Fields)
            .Include(template => template.ApprovalSteps)
            .SingleOrDefaultAsync(template => template.Id == id, cancellationToken);

        return form is null ? NotFound() : Ok(ToResponse(form));
    }

    [HttpPost("{id:int}/approval-steps")]
    public async Task<ActionResult<FormTemplateResponse>> AddApprovalSteps(
        int id,
        AddApprovalStepsRequest request,
        CancellationToken cancellationToken)
    {
        var form = await db.FormTemplates
            .Include(template => template.Fields)
            .Include(template => template.ApprovalSteps)
            .SingleOrDefaultAsync(template => template.Id == id, cancellationToken);

        if (form is null)
        {
            return NotFound();
        }

        form.ApprovalSteps.Clear();
        form.ApprovalSteps.AddRange(request.ApprovalSteps.Select((step, index) => new ApprovalStep
        {
            StepOrder = index + 1,
            Name = step.Name.Trim(),
            ApproverId = step.ApproverId.Trim(),
            ActionType = step.ActionType
        }));

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(form));
    }

    private static FormTemplateResponse ToResponse(FormTemplate form) => new(
        form.Id,
        form.Name,
        form.CreatedAtUtc,
        form.CreatedBy,
        form.Fields.OrderBy(field => field.DisplayOrder).Select(field => new FormFieldResponse(
            field.Id, field.Label, field.FieldType.ToString(), field.DisplayOrder, field.IsRequired)).ToList(),
        form.ApprovalSteps.OrderBy(step => step.StepOrder).Select(step => new ApprovalStepResponse(
            step.Id, step.StepOrder, step.Name, step.ApproverId, step.ActionType)).ToList());

    private static FormTemplateResponse ToResponseQuery(FormTemplate form) => new(
        form.Id,
        form.Name,
        form.CreatedAtUtc,
        form.CreatedBy,
        form.Fields.OrderBy(field => field.DisplayOrder).Select(field => new FormFieldResponse(
            field.Id, field.Label, field.FieldType.ToString(), field.DisplayOrder, field.IsRequired)).ToList(),
        form.ApprovalSteps.OrderBy(step => step.StepOrder).Select(step => new ApprovalStepResponse(
            step.Id, step.StepOrder, step.Name, step.ApproverId, step.ActionType)).ToList());
}
