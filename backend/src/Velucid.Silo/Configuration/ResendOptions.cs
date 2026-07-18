namespace Velucid.Silo.Configuration;

/// <summary>
/// Configuration options for the Resend transactional email provider.
/// When <see cref="ApiKey"/> is null or empty, the email service falls back to
/// <c>ConsoleEmailService</c> (development mode) which logs messages to stdout.
/// </summary>
public sealed class ResendOptions
{
    /// <summary>
    /// The configuration section name used in appsettings.json.
    /// </summary>
    public const string SectionName = "Resend";

    /// <summary>
    /// The Resend API key. When unset, the silo uses <c>ConsoleEmailService</c> instead.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The default sender address used for all transactional emails.
    /// Format: <c>"Display Name &lt;address@example.com&gt;"</c>.
    /// </summary>
    public string FromAddress { get; set; } = "Velucid <noreply@velucid.dev>";
}
