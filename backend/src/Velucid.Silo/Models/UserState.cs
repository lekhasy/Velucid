namespace Velucid.Silo.Models;

/// <summary>
/// The aggregate state for the User grain, rebuilt by replaying events from
/// the <c>user-{email}</c> KurrentDB stream. Note: the OTP cache is NOT part
/// of this state — it lives in Redis via <see cref="Services.IOtpStore"/>.
/// </summary>
public class UserState
{
    /// <summary>
    /// The unique identifier for the user. Null until first successful
    /// OTP verify (i.e., until the <see cref="Events.UserCreatedEvent"/>
    /// has been emitted).
    /// </summary>
    public Guid? UserId { get; internal set; }

    /// <summary>
    /// The user's display name. Initialized from the email local part on
    /// first sign-in; can be changed via <c>UpdateProfile</c>.
    /// </summary>
    public string DisplayName { get; internal set; } = string.Empty;

    /// <summary>
    /// The URL of the user's avatar. Null until the user sets one.
    /// </summary>
    public string? AvatarUrl { get; internal set; }

    /// <summary>
    /// When the user was first created (null until first verify).
    /// </summary>
    public DateTimeOffset? CreatedAt { get; internal set; }

    /// <summary>
    /// When the user last successfully signed in. Null until first verify.
    /// </summary>
    public DateTimeOffset? LastSignInAt { get; internal set; }

    /// <summary>
    /// Total successful sign-ins for this user.
    /// </summary>
    public int SignInCount { get; internal set; }
}
