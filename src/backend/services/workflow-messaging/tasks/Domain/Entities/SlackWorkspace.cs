using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class SlackWorkspace : AggregateRoot<Guid>
{
    private SlackWorkspace(
        Guid id,
        Guid siteId,
        string workspaceId,
        string workspaceName,
        string? encryptedBotToken,
        string? encryptedRefreshToken,
        bool isActive,
        Guid installedByUserId,
        DateTimeOffset installedAt,
        DateTimeOffset? lastVerifiedAt) : base(id)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceId));
        }

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new ArgumentException("Workspace name is required.", nameof(workspaceName));
        }

        if (installedByUserId == Guid.Empty)
        {
            throw new ArgumentException("InstalledBy user identifier is required.", nameof(installedByUserId));
        }

        SiteId = siteId;
        WorkspaceId = workspaceId.Trim();
        WorkspaceName = workspaceName.Trim();
        EncryptedBotToken = string.IsNullOrWhiteSpace(encryptedBotToken) ? null : encryptedBotToken;
        EncryptedRefreshToken = string.IsNullOrWhiteSpace(encryptedRefreshToken) ? null : encryptedRefreshToken;
        IsActive = isActive;
        InstalledByUserId = installedByUserId;
        InstalledAt = installedAt;
        LastVerifiedAt = lastVerifiedAt;
    }

    public Guid SiteId { get; }
    public string WorkspaceId { get; private set; }
    public string WorkspaceName { get; private set; }
    public string? EncryptedBotToken { get; private set; }
    public string? EncryptedRefreshToken { get; private set; }
    public bool IsActive { get; private set; }
    public Guid InstalledByUserId { get; }
    public DateTimeOffset InstalledAt { get; }
    public DateTimeOffset? LastVerifiedAt { get; private set; }

    public static SlackWorkspace Create(
        Guid siteId,
        string workspaceId,
        string workspaceName,
        string? encryptedBotToken,
        string? encryptedRefreshToken,
        Guid installedByUserId,
        DateTimeOffset? installedAt = null)
    {
        var timestamp = installedAt ?? DateTimeOffset.UtcNow;
        return new SlackWorkspace(
            Guid.NewGuid(),
            siteId,
            workspaceId,
            workspaceName,
            encryptedBotToken,
            encryptedRefreshToken,
            isActive: true,
            installedByUserId,
            timestamp,
            lastVerifiedAt: null);
    }

    public static SlackWorkspace FromPersistence(
        Guid id,
        Guid siteId,
        string workspaceId,
        string workspaceName,
        string? encryptedBotToken,
        string? encryptedRefreshToken,
        bool isActive,
        Guid installedByUserId,
        DateTimeOffset installedAt,
        DateTimeOffset? lastVerifiedAt)
    {
        return new SlackWorkspace(
            id,
            siteId,
            workspaceId,
            workspaceName,
            encryptedBotToken,
            encryptedRefreshToken,
            isActive,
            installedByUserId,
            installedAt,
            lastVerifiedAt);
    }

    public void UpdateMetadata(string workspaceName, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new ArgumentException("Workspace name is required.", nameof(workspaceName));
        }

        WorkspaceName = workspaceName.Trim();
        IsActive = isActive;
    }

    public void UpdateSecrets(string? encryptedBotToken, string? encryptedRefreshToken)
    {
        EncryptedBotToken = string.IsNullOrWhiteSpace(encryptedBotToken) ? EncryptedBotToken : encryptedBotToken;
        EncryptedRefreshToken = string.IsNullOrWhiteSpace(encryptedRefreshToken) ? EncryptedRefreshToken : encryptedRefreshToken;
    }

    public void Verify(DateTimeOffset verifiedAt)
    {
        LastVerifiedAt = verifiedAt;
    }
}
