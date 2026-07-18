using Orleans;

namespace Velucid.Silo.Models;

/// <summary>
/// Returned to the BFF after a successful OTP verify. Carries the user id
/// (used to mint the session cookie) and a flag indicating first-time
/// sign-in (used to route to /onboarding).
/// </summary>
[GenerateSerializer]
[Immutable]
public record LoginResult(
    [property: Id(0)] Guid UserId,
    [property: Id(1)] string Email,
    [property: Id(2)] string DisplayName,
    [property: Id(3)] string? AvatarUrl,
    [property: Id(4)] bool IsNewUser
);
