using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Standard Operating Procedure (SOP) entity representing organizational procedures
/// that can be attached to tasks. SOPs are scoped at the organization level.
/// </summary>
public sealed class StandardOperatingProcedure : AggregateRoot<Guid>
{
    private StandardOperatingProcedure(
        Guid id,
        Guid orgId,
        string title,
        string? content,
        string? category,
        int version,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("Organization identifier is required.", nameof(orgId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by identifier is required.", nameof(createdByUserId));

        OrgId = orgId;
        Title = title.Trim();
        Content = content?.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Version = version;
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid OrgId { get; }
    public string Title { get; private set; }
    public string? Content { get; private set; }
    public string? Category { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static StandardOperatingProcedure Create(
        Guid orgId,
        string title,
        string? content,
        string? category,
        Guid createdByUserId)
    {
        var now = DateTimeOffset.UtcNow;
        return new StandardOperatingProcedure(
            Guid.NewGuid(),
            orgId,
            title,
            content,
            category,
            version: 1,
            isActive: true,
            createdByUserId,
            now,
            now);
    }

    public static StandardOperatingProcedure FromPersistence(
        Guid id,
        Guid orgId,
        string title,
        string? content,
        string? category,
        int version,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new StandardOperatingProcedure(
            id,
            orgId,
            title,
            content,
            category,
            version,
            isActive,
            createdByUserId,
            createdAt,
            updatedAt);
    }

    public void Update(string title, string? content, string? category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Content = content?.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

