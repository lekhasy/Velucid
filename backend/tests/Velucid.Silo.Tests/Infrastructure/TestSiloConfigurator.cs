using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;
using Velucid.ReadModel;
using Velucid.Silo.Authorization;
using Velucid.Silo.Events;
using Velucid.Silo.Services;

namespace Velucid.Silo.Tests.Infrastructure;

/// <summary>
/// Configures the Orleans test silo with in-memory event stream client,
/// time provider, OTP store, email service, and read model.
/// </summary>
public sealed class TestSiloConfigurator : ISiloConfigurator
{
    /// <summary>
    /// Shared in-memory event stream client used across the test silo.
    /// Set before the test cluster is created.
    /// </summary>
    internal static InMemoryEventStreamClient? SharedEventStreamClient;

    /// <summary>
    /// Shared time provider for controlling time in tests.
    /// Set before the test cluster is created. Defaults to <see cref="TimeProvider.System"/>.
    /// </summary>
    internal static TimeProvider? SharedTimeProvider;

    /// <summary>
    /// Shared OTP store for tests. Defaults to a no-op stub.
    /// </summary>
    internal static IOtpStore SharedOtpStore = new NoopOtpStore();

    /// <summary>
    /// Shared email service for tests. Defaults to a recording stub that
    /// captures messages instead of sending them.
    /// </summary>
    internal static IEmailService SharedEmailService = new RecordingEmailService();

    internal static InMemoryOpenFgaAuthorizationService? SharedAuthService;

    /// <inheritdoc/>
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IEventStreamClient>(
                _ => SharedEventStreamClient
                     ?? throw new InvalidOperationException(
                         "SharedEventStreamClient must be set before starting the test cluster."));

            services.AddSingleton(_ => SharedTimeProvider ?? TimeProvider.System);

            services.AddSingleton<IOtpStore>(_ => SharedOtpStore);
            services.AddSingleton<IEmailService>(_ => SharedEmailService);

            var authService = SharedAuthService ?? new InMemoryOpenFgaAuthorizationService();
            services.AddSingleton<IOpenFgaAuthorizationService>(authService);

            services.AddDbContext<ReadModelDbContext>(options =>
                options.UseInMemoryDatabase("VelucidTestReadModel"));
        });
    }
}

/// <summary>
/// Records emails sent through <see cref="IEmailService"/> so tests can
/// assert on them. Always returns success.
/// </summary>
public sealed class RecordingEmailService : IEmailService
{
    public List<SentOtp> Otps { get; } = new();
    public List<SentWorkspaceInvitation> WorkspaceInvitations { get; } = new();
    public List<SentWorkItemAssigned> WorkItemAssigned { get; } = new();

    public Task SendOtpCodeAsync(string to, string code, TimeSpan codeLifetime, CancellationToken ct = default)
    {
        Otps.Add(new SentOtp(to, code, codeLifetime));
        return Task.CompletedTask;
    }

    public Task SendWorkspaceInvitationAsync(string to, string workspaceName, string inviteCode, string acceptUrl, CancellationToken ct = default)
    {
        WorkspaceInvitations.Add(new SentWorkspaceInvitation(to, workspaceName, inviteCode, acceptUrl));
        return Task.CompletedTask;
    }

    public Task SendWorkItemAssignedAsync(string to, string workItemTitle, string workspaceName, string workItemUrl, CancellationToken ct = default)
    {
        WorkItemAssigned.Add(new SentWorkItemAssigned(to, workItemTitle, workspaceName, workItemUrl));
        return Task.CompletedTask;
    }
}

public sealed record SentOtp(string To, string Code, TimeSpan Lifetime);
public sealed record SentWorkspaceInvitation(string To, string WorkspaceName, string InviteCode, string AcceptUrl);
public sealed record SentWorkItemAssigned(string To, string WorkItemTitle, string WorkspaceName, string WorkItemUrl);

/// <summary>
/// No-op OTP store. Tests that exercise the OTP code flow should replace
/// this with a real (in-memory) implementation.
/// </summary>
public sealed class NoopOtpStore : IOtpStore
{
    public Task SetAsync(string email, OtpCode code, CancellationToken ct = default) => Task.CompletedTask;
    public Task<OtpCode?> GetAsync(string email, CancellationToken ct = default) => Task.FromResult<OtpCode?>(null);
    public Task<int> IncrementAttemptAsync(string email, CancellationToken ct = default) => Task.FromResult(0);
    public Task DeleteAsync(string email, CancellationToken ct = default) => Task.CompletedTask;
}
