namespace Velucid.Silo.Services;

/// <summary>
/// HTML and plain-text email templates. Kept inline (not in a separate
/// template engine) so the silo has zero extra dependencies. The look is
/// intentionally minimal and matches Velucid's landing-page register.
/// </summary>
internal static class OtpEmailTemplate
{
    public static string Html(string code, int expiresInMinutes) => $$"""
        <!doctype html>
        <html>
          <body style="margin:0;padding:0;background:#f8fafc;font-family:Inter,system-ui,-apple-system,sans-serif;color:#0f172a;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f8fafc;padding:40px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:440px;background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;padding:32px;">
                    <tr>
                      <td>
                        <div style="font-size:20px;font-weight:700;letter-spacing:-0.01em;margin-bottom:24px;">Velucid</div>
                        <div style="font-size:15px;line-height:1.55;margin-bottom:24px;">
                          Your sign-in code:
                        </div>
                        <div style="font-size:32px;font-weight:700;letter-spacing:0.16em;font-family:'JetBrains Mono',ui-monospace,SFMono-Regular,Menlo,monospace;padding:20px 24px;background:#0f172a;color:#ffffff;border-radius:8px;text-align:center;margin-bottom:24px;">
                          {{code}}
                        </div>
                        <div style="font-size:13px;line-height:1.55;color:#475569;margin-bottom:24px;">
                          This code expires in {{expiresInMinutes}} minute{{(expiresInMinutes == 1 ? "" : "s")}}. If you didn't request this, you can ignore the message.
                        </div>
                        <div style="font-size:12px;color:#94a3b8;border-top:1px solid #e2e8f0;padding-top:16px;">
                          Velucid &middot; sign in
                        </div>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    public static string Text(string code, int expiresInMinutes) => $"""
        Velucid

        Your sign-in code: {code}

        This code expires in {expiresInMinutes} minute{(expiresInMinutes == 1 ? "" : "s")}. If you didn't request this, you can ignore the message.
        """;
}

internal static class WorkspaceInvitationEmailTemplate
{
    public static string Html(string workspaceName, string inviteCode, string acceptUrl) => $$"""
        <!doctype html>
        <html>
          <body style="margin:0;padding:0;background:#f8fafc;font-family:Inter,system-ui,-apple-system,sans-serif;color:#0f172a;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f8fafc;padding:40px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:440px;background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;padding:32px;">
                    <tr>
                      <td>
                        <div style="font-size:20px;font-weight:700;letter-spacing:-0.01em;margin-bottom:24px;">Velucid</div>
                        <div style="font-size:15px;line-height:1.55;margin-bottom:8px;">
                          You've been invited to <strong>{{workspaceName}}</strong>.
                        </div>
                        <div style="font-size:13px;line-height:1.55;color:#475569;margin-bottom:24px;">
                          Use the code below after signing in to join the workspace. The code is single-use.
                        </div>
                        <div style="font-size:24px;font-weight:700;letter-spacing:0.16em;font-family:'JetBrains Mono',ui-monospace,SFMono-Regular,Menlo,monospace;padding:16px 20px;background:#0f172a;color:#ffffff;border-radius:8px;text-align:center;margin-bottom:16px;">
                          {{inviteCode}}
                        </div>
                        <div style="text-align:center;margin-bottom:24px;">
                          <a href="{{acceptUrl}}" style="display:inline-block;padding:10px 20px;background:#0f172a;color:#ffffff;border-radius:6px;text-decoration:none;font-weight:600;font-size:14px;">Accept invitation</a>
                        </div>
                        <div style="font-size:12px;color:#94a3b8;border-top:1px solid #e2e8f0;padding-top:16px;">
                          Velucid &middot; workspace invitation
                        </div>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    public static string Text(string workspaceName, string inviteCode, string acceptUrl) => $"""
        Velucid

        You've been invited to {workspaceName}.

        Use this code after signing in to join the workspace. The code is single-use.

        {inviteCode}

        Or open this link: {acceptUrl}
        """;
}

internal static class WorkItemAssignedEmailTemplate
{
    public static string Html(string workItemTitle, string workspaceName, string workItemUrl) => $$"""
        <!doctype html>
        <html>
          <body style="margin:0;padding:0;background:#f8fafc;font-family:Inter,system-ui,-apple-system,sans-serif;color:#0f172a;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f8fafc;padding:40px 16px;">
              <tr>
                <td align="center">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:440px;background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;padding:32px;">
                    <tr>
                      <td>
                        <div style="font-size:20px;font-weight:700;letter-spacing:-0.01em;margin-bottom:24px;">Velucid</div>
                        <div style="font-size:15px;line-height:1.55;margin-bottom:8px;">
                          You've been assigned a work item in <strong>{{workspaceName}}</strong>:
                        </div>
                        <div style="font-size:18px;font-weight:600;margin-bottom:24px;padding:12px 16px;background:#f1f5f9;border-radius:6px;">
                          {{workItemTitle}}
                        </div>
                        <div style="text-align:center;margin-bottom:24px;">
                          <a href="{{workItemUrl}}" style="display:inline-block;padding:10px 20px;background:#0f172a;color:#ffffff;border-radius:6px;text-decoration:none;font-weight:600;font-size:14px;">Open work item</a>
                        </div>
                        <div style="font-size:12px;color:#94a3b8;border-top:1px solid #e2e8f0;padding-top:16px;">
                          Velucid &middot; assignment notification
                        </div>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    public static string Text(string workItemTitle, string workspaceName, string workItemUrl) => $"""
        Velucid

        You've been assigned a work item in {workspaceName}:

        {workItemTitle}

        Open it: {workItemUrl}
        """;
}
