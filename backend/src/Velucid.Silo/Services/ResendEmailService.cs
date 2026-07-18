using Microsoft.Extensions.Options;
using Resend;
using Velucid.Silo.Configuration;

namespace Velucid.Silo.Services;

/// <summary>
/// Sends transactional email via the Resend HTTP API. This is the production
/// implementation of <see cref="IEmailService"/>; in development (or when
/// <c>Resend:ApiKey</c> is not configured) the silo falls back to
/// <c>ConsoleEmailService</c>.
/// </summary>
public sealed class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IOptions<ResendOptions> options,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendOtpCodeAsync(string to, string code, TimeSpan codeLifetime, CancellationToken ct = default)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(codeLifetime.TotalMinutes));
        var subject = "Your Velucid sign-in code";
        var html = OtpEmailTemplate.Html(code, minutes);
        var text = OtpEmailTemplate.Text(code, minutes);

        await SendAsync(to, subject, html, text, ct);
    }

    public async Task SendWorkspaceInvitationAsync(
        string to,
        string workspaceName,
        string inviteCode,
        string acceptUrl,
        CancellationToken ct = default)
    {
        var subject = $"You've been invited to {workspaceName} on Velucid";
        var html = WorkspaceInvitationEmailTemplate.Html(workspaceName, inviteCode, acceptUrl);
        var text = WorkspaceInvitationEmailTemplate.Text(workspaceName, inviteCode, acceptUrl);

        await SendAsync(to, subject, html, text, ct);
    }

    public async Task SendWorkItemAssignedAsync(
        string to,
        string workItemTitle,
        string workspaceName,
        string workItemUrl,
        CancellationToken ct = default)
    {
        var subject = $"Assigned to you in {workspaceName}: {workItemTitle}";
        var html = WorkItemAssignedEmailTemplate.Html(workItemTitle, workspaceName, workItemUrl);
        var text = WorkItemAssignedEmailTemplate.Text(workItemTitle, workspaceName, workItemUrl);

        await SendAsync(to, subject, html, text, ct);
    }

    private async Task SendAsync(string to, string subject, string html, string text, CancellationToken ct)
    {
        var message = new EmailMessage
        {
            From = _options.FromAddress,
            To = new[] { to },
            Subject = subject,
            HtmlBody = html,
            TextBody = text,
        };

        try
        {
            var response = await _resend.EmailSendAsync(message, ct);
            _logger.LogInformation("Sent email {Subject} to {To} (Resend id: {Id})", subject, to, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email {Subject} to {To}", subject, to);
            throw;
        }
    }
}
