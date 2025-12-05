using System;
using System.Collections.Generic;
using FluentValidation;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class PostMessageRequest
{
    public string Content { get; init; } = string.Empty;
    public Guid? ParentMessageId { get; init; }
    public string? MetadataJson { get; init; }
    public IReadOnlyCollection<PostMessageAttachmentRequest>? Attachments { get; init; }
}

public sealed class PostMessageAttachmentRequest
{
    public MessageAttachmentType AttachmentType { get; init; }
    public string FileUrl { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
    public string? MimeType { get; init; }
    public string? MetadataJson { get; init; }
}

public sealed class PostMessageRequestValidator : AbstractValidator<PostMessageRequest>
{
    public PostMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotNull()
            .WithMessage("Content must be provided when no attachments are included.")
            .Must(content => !string.IsNullOrWhiteSpace(content))
            .WithMessage("Content cannot be empty or whitespace-only when no attachments are included.")
            .When(x => x.Attachments is null || x.Attachments.Count == 0);

        RuleFor(x => x.Attachments)
            .Must(attachments => attachments != null && attachments.Count > 0)
            .WithMessage("At least one attachment must be provided when content is empty.")
            .When(x => string.IsNullOrWhiteSpace(x.Content));

        RuleFor(x => x.Attachments)
            .Must(attachments => attachments != null && attachments.Count > 0)
            .WithMessage("Attachments collection must contain at least one item when provided.")
            .When(x => x.Attachments != null);

        RuleForEach(x => x.Attachments)
            .SetValidator(new PostMessageAttachmentRequestValidator())
            .When(x => x.Attachments != null);

        RuleFor(x => x.ParentMessageId)
            .Must(parentId => parentId != Guid.Empty)
            .WithMessage("ParentMessageId cannot be an empty GUID when provided.")
            .When(x => x.ParentMessageId.HasValue);
    }
}

public sealed class PostMessageAttachmentRequestValidator : AbstractValidator<PostMessageAttachmentRequest>
{
    public PostMessageAttachmentRequestValidator()
    {
        RuleFor(x => x.FileUrl)
            .NotNull()
            .WithMessage("FileUrl is required.")
            .NotEmpty()
            .WithMessage("FileUrl cannot be empty.")
            .Must(url => !string.IsNullOrWhiteSpace(url))
            .WithMessage("FileUrl cannot be whitespace-only.");
    }
}
