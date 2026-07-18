namespace Velucid.Silo.Services;

/// <summary>
/// One-time code issued during email-OTP sign-in. The plain code is generated
/// in the grain and only ever sent via email — only the SHA-256 hash lands
/// in Redis. Codes are single-use and time-bounded; rate-limit state is
/// derived from <see cref="IssuedAt"/> and the configured cooldown.
/// </summary>
public sealed class OtpCode
{
    public required string CodeHash { get; init; }
    public required string Email { get; init; }
    public required DateTimeOffset IssuedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string RequestIp { get; init; }
    public int AttemptCount { get; set; }
}

/// <summary>
/// Redis-backed store for email-OTP codes. Keys are scoped per email; one
/// active code per email at a time. Re-issuing overwrites the prior code.
/// </summary>
public interface IOtpStore
{
    /// <summary>
    /// Persists a freshly-issued code (hashed) with the configured TTL.
    /// Overwrites any existing code for the same email.
    /// </summary>
    Task SetAsync(string email, OtpCode code, CancellationToken ct = default);

    /// <summary>
    /// Returns the active code for the email, or null if none exists or the
    /// stored code has expired.
    /// </summary>
    Task<OtpCode?> GetAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Atomically increments the attempt counter for the stored code.
    /// Returns the new attempt count. Returns 0 if no code exists.
    /// </summary>
    Task<int> IncrementAttemptAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Removes the active code (called on successful verification or when
    /// invalidating a code after too many failed attempts).
    /// </summary>
    Task DeleteAsync(string email, CancellationToken ct = default);
}
