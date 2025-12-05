using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for CreatePhenotypeRequest
/// </summary>
public sealed class CreatePhenotypeRequestValidator : AbstractValidator<CreatePhenotypeRequest>
{
    public CreatePhenotypeRequestValidator()
    {
        RuleFor(x => x.GeneticsId)
            .NotEmpty()
            .WithMessage("Genetics ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Phenotype name is required.")
            .MaximumLength(200)
            .WithMessage("Phenotype name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.ExpressionNotes)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrWhiteSpace(x.ExpressionNotes))
            .WithMessage("Expression notes cannot exceed 5000 characters.");
    }
}
