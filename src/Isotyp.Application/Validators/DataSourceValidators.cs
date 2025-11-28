using FluentValidation;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Validators;

public class CreateDataSourceDtoValidator : AbstractValidator<CreateDataSourceDto>
{
    public CreateDataSourceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ConnectionStringReference)
            .NotEmpty().WithMessage("Connection string reference is required.")
            .MaximumLength(500).WithMessage("Connection string reference must not exceed 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid data source type.");
    }
}

public class UpdateDataSourceDtoValidator : AbstractValidator<UpdateDataSourceDto>
{
    public UpdateDataSourceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}
