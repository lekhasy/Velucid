using System.Security.Cryptography;
using System.Text;
using Velucid.Silo.Events;
using Velucid.Silo.Models;
using Velucid.Silo.Services;

namespace Velucid.Silo.Grains;

/// <summary>
/// Event-sourced grain that owns the email-OTP sign-in flow and the User
/// aggregate. Keyed by the lowercased email address; persists events to
/// the <c>user-{email}</c> KurrentDB stream. The OTP cache itself is
/// stored in Redis via <see cref="IOtpStore"/> and is NOT part of the
/// event log — only audit events (<see cref="OtpRequestedEvent"/>,
/// <see cref="OtpVerifiedEvent"/>, <see cref="UserCreatedEvent"/>) are.
/// </summary>
public class UserGrain : EventSourcedGrain<UserState>, IUserGrain
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;

    private readonly IOtpStore _otpStore;
    private readonly IEmailService _emailService;

    public UserGrain(
        IEventStreamClient eventStreamClient,
        TimeProvider timeProvider,
        IOtpStore otpStore,
        IEmailService emailService)
        : base(eventStreamClient, timeProvider)
    {
        _otpStore = otpStore;
        _emailService = emailService;
    }

    /// <inheritdoc/>
    protected override string BuildStreamId()
    {
        // The grain key is the lowercased email; the stream name is "user-{key}".
        return $"user-{this.GetPrimaryKeyString()}";
    }

    /// <inheritdoc/>
    protected override void Apply(UserState state, IEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e:
                state.UserId = e.UserId;
                state.DisplayName = e.DisplayName;
                state.AvatarUrl = e.AvatarUrl;
                state.CreatedAt = e.Timestamp;
                state.LastSignInAt = e.Timestamp;
                state.SignInCount = 1;
                break;

            case OtpVerifiedEvent e:
                state.LastSignInAt = e.Timestamp;
                state.SignInCount += 1;
                break;

            case UserProfileUpdatedEvent e:
                state.DisplayName = e.DisplayName;
                state.AvatarUrl = e.AvatarUrl;
                break;
        }
    }

    /// <inheritdoc/>
    public async Task RequestOtpCodeAsync(string requestIp)
    {
        var email = this.GetPrimaryKeyString();

        // Cooldown: don't issue a new code if a fresh one already exists.
        var existing = await _otpStore.GetAsync(email);
        if (existing is not null && existing.IssuedAt + ResendCooldown > UtcNow)
        {
            throw new InvalidOperationException(
                "A code was sent recently. Please wait a moment before requesting another.");
        }

        // Generate 6-digit code, hash it, store in Redis.
        var code = Random.Shared.Next(100_000, 1_000_000).ToString("D6");
        var codeHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
        var expiresAt = UtcNow + OtpLifetime;

        var otpCode = new OtpCode
        {
            CodeHash = codeHash,
            Email = email,
            IssuedAt = UtcNow,
            ExpiresAt = expiresAt,
            RequestIp = requestIp,
        };

        // Send the email BEFORE persisting. If sending fails, no event is
        // emitted and Redis is not written — the user gets a clean retry.
        await _emailService.SendOtpCodeAsync(email, code, OtpLifetime);

        await _otpStore.SetAsync(email, otpCode);

        await EmitEvent(new OtpRequestedEvent(
            email, codeHash, expiresAt, requestIp,
            ActorId: State.UserId ?? Guid.Empty,
            Timestamp: UtcNow));
    }

    /// <inheritdoc/>
    public async Task<LoginResult> VerifyOtpCodeAsync(string code)
    {
        var email = this.GetPrimaryKeyString();
        var stored = await _otpStore.GetAsync(email);

        if (stored is null)
        {
            throw new InvalidOperationException(
                "No active sign-in code. Request a new code first.");
        }

        // Increment attempt counter before comparing so brute-force is logged
        // and rate-limited even when the attacker probes the wrong code.
        var attempts = await _otpStore.IncrementAttemptAsync(email);
        if (attempts > MaxAttempts)
        {
            await _otpStore.DeleteAsync(email);
            throw new InvalidOperationException("Too many attempts. Request a new code.");
        }

        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        var storedHash = Convert.FromBase64String(stored.CodeHash);

        if (!CryptographicOperations.FixedTimeEquals(suppliedHash, storedHash))
        {
            throw new InvalidOperationException("Invalid code.");
        }

        // Code is correct — single-use, delete it now.
        await _otpStore.DeleteAsync(email);

        var isNewUser = State.UserId is null;
        var userId = State.UserId ?? Guid.NewGuid();
        var now = UtcNow;

        if (isNewUser)
        {
            // Derive a starter display name from the email local part.
            // The user can change it during onboarding or later via UpdateProfile.
            var localPart = email.Split('@')[0];
            var displayName = string.IsNullOrWhiteSpace(localPart) ? email : localPart;

            await EmitEvent(new UserCreatedEvent(
                userId, email, displayName, AvatarUrl: null,
                ActorId: userId, Timestamp: now));
        }

        await EmitEvent(new OtpVerifiedEvent(
            userId, email,
            ActorId: userId, Timestamp: now));

        return new LoginResult(
            UserId: userId,
            Email: email,
            DisplayName: State.DisplayName,
            AvatarUrl: State.AvatarUrl,
            IsNewUser: isNewUser);
    }

    /// <inheritdoc/>
    public async Task UpdateProfile(string displayName, string avatarUrl)
    {
        if (State.UserId is null)
            throw new InvalidOperationException("User has not completed sign-in.");

        if (State.DisplayName == displayName && State.AvatarUrl == avatarUrl)
            return;

        await EmitEvent(new UserProfileUpdatedEvent(
            State.UserId.Value, displayName, avatarUrl,
            State.UserId.Value, UtcNow));
    }

    /// <inheritdoc/>
    public Task<UserInfo> GetUserInfo()
    {
        if (State.UserId is null)
            throw new InvalidOperationException("User has not completed sign-in.");

        return Task.FromResult(new UserInfo(
            State.UserId.Value,
            State.DisplayName,
            State.AvatarUrl ?? string.Empty,
            State.UserId is null ? null : this.GetPrimaryKeyString(),
            IsEmailVerified: true));
    }
}
