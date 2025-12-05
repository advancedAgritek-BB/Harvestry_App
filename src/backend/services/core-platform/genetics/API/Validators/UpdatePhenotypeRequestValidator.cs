using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for UpdatePhenotypeRequest
/// </summary>
public sealed class UpdatePhenotypeRequestValidator : AbstractValidator<UpdatePhenotypeRequest>
{
    public UpdatePhenotypeRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.ExpressionNotes)
            .MaximumLength(5000)
            .WithMessage("Expression notes cannot exceed 5000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ExpressionNotes));
    }
}

