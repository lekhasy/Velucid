using Velucid.Silo.Models;

namespace Velucid.Silo.Grains;

/// <summary>
/// Grain interface for the User aggregate. Keyed by the lowercased email
/// address (e.g., <c>grainFactory.GetGrain&lt;IUserGrain&gt;("alice@example.com")</c>).
/// The grain exists from the moment the first OTP code is issued; the user
/// record is materialized on the first successful verify.
/// </summary>
/// <remarks>
/// Persists events to the <c>user-{email}</c> KurrentDB stream. The OTP
/// state itself is not event-sourced — it lives in Redis via
/// <see cref="Services.IOtpStore"/> with a 10-minute TTL.
/// </remarks>
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Issues a fresh 6-digit OTP code, stores its hash in Redis with a TTL,
    /// and sends the plain code via email. Throws if a code was issued within
    /// the cooldown window for this email. Throws if the email service call
    /// fails (no event is emitted in that case).
    /// </summary>
    /// <param name="requestIp">The IP address that requested the code (for rate limiting / audit).</param>
    Task RequestOtpCodeAsync(string requestIp);

    /// <summary>
    /// Verifies the supplied 6-digit code against the stored hash, using a
    /// constant-time compare. On first-time sign-in, the user is created
    /// (a stable <c>UserId</c> is generated) and the
    /// <see cref="Events.UserCreatedEvent"/> is emitted. On every successful
    /// verify, an <see cref="Events.OtpVerifiedEvent"/> is emitted. Returns
    /// the post-verify login info.
    /// </summary>
    /// <param name="code">The 6-digit code the user entered.</param>
    /// <returns>The login result with the user id and a flag indicating first-time sign-in.</returns>
    /// <exception cref="InvalidOperationException">No active code, code expired, too many attempts, or wrong code.</exception>
    Task<LoginResult> VerifyOtpCodeAsync(string code);

    /// <summary>
    /// Updates the user's profile display name and avatar URL. Throws if the
    /// user has not yet completed first-time sign-in.
    /// </summary>
    Task UpdateProfile(string displayName, string avatarUrl);

    /// <summary>
    /// Returns the user's public info. Throws if the user has not yet completed first-time sign-in.
    /// </summary>
    Task<UserInfo> GetUserInfo();
}
