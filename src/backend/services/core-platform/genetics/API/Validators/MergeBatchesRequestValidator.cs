using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class MergeBatchesRequestValidator : AbstractValidator<MergeBatchesRequest>
{
    public MergeBatchesRequestValidator()
    {
        RuleFor(x => x.SourceBatchIds)
            .NotEmpty().WithMessage("Source batch IDs are required")
            .Must(ids => ids.Length >= 2).WithMessage("At least 2 source batches are required for merge")
            .Must(ids => ids.Length <= 50).WithMessage("Cannot merge more than 50 batches at once");

        RuleFor(x => x.MergedBatchName)
            .NotEmpty().WithMessage("Merged batch name is required")
            .MaximumLength(200).WithMessage("Merged batch name cannot exceed 200 characters");

        When(x => !string.IsNullOrWhiteSpace(x.MergeReason), () =>
        {
            RuleFor(x => x.MergeReason!)
                .MaximumLength(500).WithMessage("Merge reason cannot exceed 500 characters");
        });
    }
}

