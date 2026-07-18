namespace Velucid.Silo.Events;

/// <summary>
/// Raised the first time a User is created via email-OTP sign-in. After this
/// event, the user has a stable <see cref="UserId"/> that all future events
/// (sign-ins, profile updates, etc.) reference.
/// </summary>
/// <param name="UserId">The unique identifier assigned to the new user.</param>
/// <param name="Email">The user's email address (also the grain key).</param>
/// <param name="DisplayName">Initial display name (derived from the email local part; user can change it in onboarding).</param>
/// <param name="AvatarUrl">Initial avatar URL (null in MVP — the user can set one later).</param>
/// <param name="ActorId">Equals <see cref="UserId"/> — the user themselves.</param>
/// <param name="Timestamp">The UTC timestamp when the event occurred.</param>
public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    Guid ActorId,
    DateTimeOffset Timestamp
) : IEvent;
