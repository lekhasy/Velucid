using System.Text.Json;
using StackExchange.Redis;

namespace Velucid.Silo.Services;

/// <summary>
/// Redis implementation of <see cref="IOtpStore"/>. Reuses the existing
/// <see cref="IConnectionMultiplexer"/> registered for Orleans clustering.
/// Key namespace: <c>otp:{emailLowercased}</c>.
/// </summary>
public sealed class RedisOtpStore : IOtpStore
{
    private const string KeyPrefix = "otp:";

    private readonly IConnectionMultiplexer _redis;

    public RedisOtpStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetAsync(string email, OtpCode code, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = Key(email);
        var json = JsonSerializer.SerializeToUtf8Bytes(code);
        // EXPIRE aligns the TTL to the code's own ExpiresAt so the stored
        // entry vanishes exactly when the code is no longer valid.
        await db.StringSetAsync(key, json, code.ExpiresAt - DateTimeOffset.UtcNow);
    }

    public async Task<OtpCode?> GetAsync(string email, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = Key(email);
        var json = (byte[]?)await db.StringGetAsync(key);

        if (json is null) return null;

        var code = JsonSerializer.Deserialize<OtpCode>((ReadOnlySpan<byte>)json);
        if (code is null || code.ExpiresAt < DateTimeOffset.UtcNow)
        {
            // Defensive: TTL should have reaped this already, but if a clock
            // skew puts us past ExpiresAt before Redis noticed, drop it.
            await db.KeyDeleteAsync(key);
            return null;
        }

        return code;
    }

    public async Task<int> IncrementAttemptAsync(string email, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = Key(email);
        // We don't atomically mutate the embedded AttemptCount; instead we
        // bump a parallel counter and read it back. Simple, race-free within
        // a single Redis connection, and avoids rewriting the JSON payload.
        var attempts = await db.StringIncrementAsync($"{key}:attempts");
        // Mirror the parent key's TTL so the attempts counter doesn't outlive it.
        var ttl = await db.KeyTimeToLiveAsync(key);
        if (ttl is not null) await db.KeyExpireAsync($"{key}:attempts", ttl);
        return (int)attempts;
    }

    public async Task DeleteAsync(string email, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = Key(email);
        await db.KeyDeleteAsync(new RedisKey[] { key, $"{key}:attempts" });
    }

    private static string Key(string email) => $"{KeyPrefix}{email.Trim().ToLowerInvariant()}";
}
