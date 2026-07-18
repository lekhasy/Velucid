namespace Velucid.Silo.Services;

/// <summary>
/// Development fallback for <see cref="IEmailService"/>. Logs the full email
/// payload to the server console. Selected automatically when
/// <c>Resend:ApiKey</c> is unset. Useful for local dev, tests, and CI.
/// </summary>
public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpCodeAsync(string to, string code, TimeSpan codeLifetime, CancellationToken ct = default)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(codeLifetime.TotalMinutes));
        _logger.LogInformation(
            "[DEV EMAIL] OTP code to {To}: code={Code}, expires_in_minutes={Minutes}",
            to, code, minutes);
        return Task.CompletedTask;
    }

    public Task SendWorkspaceInvitationAsync(
        string to,
        string workspaceName,
        string inviteCode,
        string acceptUrl,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Workspace invitation to {To}: workspace={Workspace}, code={Code}, url={Url}",
            to, workspaceName, inviteCode, acceptUrl);
        return Task.CompletedTask;
    }

    public Task SendWorkItemAssignedAsync(
        string to,
        string workItemTitle,
        string workspaceName,
        string workItemUrl,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Work item assigned to {To}: title={Title}, workspace={Workspace}, url={Url}",
            to, workItemTitle, workspaceName, workItemUrl);
        return Task.CompletedTask;
    }
}
