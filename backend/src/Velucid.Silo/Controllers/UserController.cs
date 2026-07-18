using Microsoft.AspNetCore.Mvc;
using Velucid.Silo.Grains;
using Velucid.Silo.Models;

namespace Velucid.Silo.Controllers;

/// <summary>
/// Email-OTP sign-in endpoints. The Astro BFF is the only legitimate caller
/// of these — the silo does not see login forms. Each request proxies into
/// the <see cref="IUserGrain"/> keyed by the lowercased email address.
/// </summary>
[ApiController]
public class UserController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;

    public UserController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    /// <summary>
    /// Issues a 6-digit OTP code, stores its hash in Redis with a 10-minute
    /// TTL, and emails the plain code to the user. Throws 409 if a code was
    /// issued within the cooldown window.
    /// </summary>
    [HttpPost("api/auth/request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var grain = _grainFactory.GetGrain<IUserGrain>(email);

        try
        {
            await grain.RequestOtpCodeAsync(GetClientIp());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Cooldown exceeded — let the caller show a friendly error.
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verifies a 6-digit code. On first-time verify, creates the user and
    /// returns IsNewUser = true so the BFF can route to /onboarding. On every
    /// successful verify, returns IsNewUser = false for the /dashboard path.
    /// </summary>
    [HttpPost("api/auth/verify-otp")]
    public async Task<ActionResult<LoginResult>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var grain = _grainFactory.GetGrain<IUserGrain>(email);

        try
        {
            var result = await grain.VerifyOtpCodeAsync(request.Code);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the user's profile. The BFF is expected to resolve the email
    /// from the session cookie and pass it in the body.
    /// </summary>
    [HttpPut("api/users/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var grain = _grainFactory.GetGrain<IUserGrain>(email);
        await grain.UpdateProfile(request.DisplayName, request.AvatarUrl);
        return Ok();
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private string GetClientIp()
    {
        // Trust X-Forwarded-For when behind the K8s ingress; fall back to remote IP.
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var fwd) && fwd.Count > 0)
        {
            return fwd[0]!.Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public record RequestOtpRequest(string Email);

public record VerifyOtpRequest(string Email, string Code);

public record UpdateProfileRequest(
    string Email,
    string DisplayName,
    string AvatarUrl);
