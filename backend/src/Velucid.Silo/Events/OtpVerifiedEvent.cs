namespace Velucid.Silo.Events;

/// <summary>
/// Raised when an OTP code is successfully verified. Always paired with a
/// <see cref="UserCreatedEvent"/> on first-time sign-in. Subsequent sign-ins
/// emit only this event.
/// </summary>
/// <param name="UserId">The identifier of the user who signed in.</param>
/// <param name="Email">The email address used to sign in.</param>
/// <param name="ActorId">Equals <see cref="UserId"/> — the user themselves.</param>
/// <param name="Timestamp">The UTC timestamp when the event occurred.</param>
public record OtpVerifiedEvent(
    Guid UserId,
    string Email,
    Guid ActorId,
    DateTimeOffset Timestamp
) : IEvent;
