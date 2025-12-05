using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class PostMessageRequestValidator : AbstractValidator<PostMessageRequest>
{
    public PostMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);

        RuleForEach(x => x.Attachments)
            .SetValidator(new PostMessageAttachmentRequestValidator());
    }

    private sealed class PostMessageAttachmentRequestValidator : AbstractValidator<PostMessageAttachmentRequest>
    {
        public PostMessageAttachmentRequestValidator()
        {
            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .MaximumLength(2048);

            RuleFor(x => x.FileName)
                .MaximumLength(255);

            RuleFor(x => x.MimeType)
                .MaximumLength(255);
        }
    }
}
