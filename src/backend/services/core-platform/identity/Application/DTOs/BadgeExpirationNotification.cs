using System;

namespace Harvestry.Identity.Application.DTOs;

public sealed record BadgeExpirationNotification(
    Guid BadgeId,
    Guid SiteId,
    Guid TargetUserId,
    string RecipientEmail,
    string RecipientName,
    string BadgeCode,
    DateTime ExpiresAt,
    bool IsManagerNotification);
