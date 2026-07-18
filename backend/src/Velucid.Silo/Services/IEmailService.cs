namespace Velucid.Silo.Services;

/// <summary>
/// Sends transactional email from the silo. Injected into grains that need
/// to send email as part of their command flow (UserGrain for OTP codes,
/// WorkspaceInvitationGrain for invites, WorkItemGrain for assignment
/// notifications, etc.).
///
/// The BFF does not call this — it is a thin passthrough. Email sending is
/// always a side effect of a grain command.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a one-time sign-in code to the supplied address. The code is
    /// short-lived (10 min default) and single-use.
    /// </summary>
    Task SendOtpCodeAsync(string to, string code, TimeSpan codeLifetime, CancellationToken ct = default);

    /// <summary>
    /// Sends a workspace invitation email with a one-time accept link/code.
    /// </summary>
    Task SendWorkspaceInvitationAsync(
        string to,
        string workspaceName,
        string inviteCode,
        string acceptUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a notification that a work item was assigned to the recipient.
    /// </summary>
    Task SendWorkItemAssignedAsync(
        string to,
        string workItemTitle,
        string workspaceName,
        string workItemUrl,
        CancellationToken ct = default);
}
