using FitnessTrackerApi.Configs;
using Microsoft.Extensions.Options;

namespace FitnessTrackerApi.Services.Auth;

public class RefreshTokenCleanupHostedService(
    ILogger<RefreshTokenCleanupHostedService> logger,
    IServiceProvider services,
    IOptions<AuthConfig> options) : BackgroundService
{
    private readonly AuthConfig _config = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupOldTokensAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(_config.RefreshTokenCleanupIntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupOldTokensAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        var tokenProvider = scope.ServiceProvider.GetService<ITokenProvider>()!;

        int numDeleted = await tokenProvider.CleanupOldTokensAsync(stoppingToken);

        logger.LogInformation("Cleaned up {NumTokens} expired refresh tokens", numDeleted);
    }
}
