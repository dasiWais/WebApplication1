using FormsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FormsApi.Data;

public sealed class FormsDbContext(DbContextOptions<FormsDbContext> options) : DbContext(options)
{
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormTemplate>(entity =>
        {
            entity.Property(form => form.Name).HasMaxLength(200).IsRequired();
            entity.Property(form => form.CreatedBy).HasMaxLength(200).IsRequired();
            entity.HasMany(form => form.Fields)
                .WithOne(field => field.FormTemplate)
                .HasForeignKey(field => field.FormTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(form => form.ApprovalSteps)
                .WithOne(step => step.FormTemplate)
                .HasForeignKey(step => step.FormTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FormField>(entity =>
        {
            entity.Property(field => field.Label).HasMaxLength(200).IsRequired();
            entity.Property(field => field.FieldType).HasConversion<int>().IsRequired();
            entity.HasIndex(field => new { field.FormTemplateId, field.DisplayOrder }).IsUnique();
        });

        modelBuilder.Entity<ApprovalStep>(entity =>
        {
            entity.Property(step => step.Name).HasMaxLength(200).IsRequired();
            entity.Property(step => step.ApproverId).HasMaxLength(200).IsRequired();
            entity.HasIndex(step => new { step.FormTemplateId, step.StepOrder }).IsUnique();
            entity.Property(step => step.ActionType).HasConversion<int>().IsRequired();
        });
    }
}
