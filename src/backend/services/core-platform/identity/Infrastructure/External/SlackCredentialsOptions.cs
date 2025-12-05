namespace Harvestry.Identity.Infrastructure.External;

public sealed class SlackCredentialsOptions
{
    public string? BotToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? WorkspaceId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
