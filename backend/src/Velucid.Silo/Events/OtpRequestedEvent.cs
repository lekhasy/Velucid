namespace Velucid.Silo.Events;

/// <summary>
/// Raised when an OTP code is issued for email-OTP sign-in. Carries the
/// SHA-256 hash of the code (never the plain text) for audit and replay.
/// The plain code is sent to the user via email and exists only in memory.
/// </summary>
/// <param name="Email">The email address the code was sent to.</param>
/// <param name="CodeHash">The SHA-256 hash of the 6-digit code.</param>
/// <param name="ExpiresAt">When the code stops being valid.</param>
/// <param name="RequestIp">The IP address that requested the code (for rate limiting / audit).</param>
/// <param name="ActorId">The identifier of the actor. For pre-sign-in events, this is <see cref="Guid.Empty"/>; after the user is created, it equals <see cref="UserId"/>.</param>
/// <param name="Timestamp">The UTC timestamp when the event occurred.</param>
public record OtpRequestedEvent(
    string Email,
    string CodeHash,
    DateTimeOffset ExpiresAt,
    string RequestIp,
    Guid ActorId,
    DateTimeOffset Timestamp
) : IEvent;
