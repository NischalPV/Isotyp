using FluentValidation;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Validators;

public class CreateSchemaVersionDtoValidator : AbstractValidator<CreateSchemaVersionDto>
{
    public CreateSchemaVersionDtoValidator()
    {
        RuleFor(x => x.DataSourceId)
            .NotEmpty().WithMessage("Data source ID is required.");

        RuleFor(x => x.SchemaDefinition)
            .NotEmpty().WithMessage("Schema definition is required.");

        RuleFor(x => x.ChangeDescription)
            .MaximumLength(5000).WithMessage("Change description must not exceed 5000 characters.");

        RuleFor(x => x.OrmMappings)
            .NotEmpty().WithMessage("ORM mappings are required.");

        RuleFor(x => x.MigrationScript)
            .NotEmpty().WithMessage("Migration script is required.");

        RuleFor(x => x.RollbackScript)
            .NotEmpty().WithMessage("Rollback script is required for safety.");
    }
}

public class CreateSchemaChangeRequestDtoValidator : AbstractValidator<CreateSchemaChangeRequestDto>
{
    public CreateSchemaChangeRequestDtoValidator()
    {
        RuleFor(x => x.SchemaVersionId)
            .NotEmpty().WithMessage("Schema version ID is required.");

        RuleFor(x => x.ChangeType)
            .IsInEnum().WithMessage("Invalid change type.");

        RuleFor(x => x.ChangeDetails)
            .NotEmpty().WithMessage("Change details are required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters.");

        RuleFor(x => x.ImpactAnalysis)
            .MaximumLength(10000).WithMessage("Impact analysis must not exceed 10000 characters.");

        RuleFor(x => x.AiConfidenceScore)
            .InclusiveBetween(0, 100)
            .When(x => x.AiConfidenceScore.HasValue)
            .WithMessage("AI confidence score must be between 0 and 100.");
    }
}

public class SubmitChangeRequestDtoValidator : AbstractValidator<SubmitChangeRequestDto>
{
    public SubmitChangeRequestDtoValidator()
    {
        RuleFor(x => x.ChangeRequestId)
            .NotEmpty().WithMessage("Change request ID is required.");

        RuleFor(x => x.Justification)
            .NotEmpty().WithMessage("Justification is required for approval.")
            .MinimumLength(50).WithMessage("Justification must be at least 50 characters.")
            .MaximumLength(5000).WithMessage("Justification must not exceed 5000 characters.");
    }
}

public class ApproveChangeRequestDtoValidator : AbstractValidator<ApproveChangeRequestDto>
{
    public ApproveChangeRequestDtoValidator()
    {
        RuleFor(x => x.ChangeRequestId)
            .NotEmpty().WithMessage("Change request ID is required.");

        RuleFor(x => x.Layer)
            .IsInEnum().WithMessage("Invalid approval layer.");

        RuleFor(x => x.ApproverUserId)
            .NotEmpty().WithMessage("Approver user ID is required.");

        RuleFor(x => x.ApproverName)
            .NotEmpty().WithMessage("Approver name is required.")
            .MaximumLength(200).WithMessage("Approver name must not exceed 200 characters.");

        RuleFor(x => x.Comments)
            .MaximumLength(5000).WithMessage("Comments must not exceed 5000 characters.");
    }
}
