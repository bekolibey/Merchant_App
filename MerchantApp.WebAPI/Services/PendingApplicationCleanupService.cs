namespace MerchantApp.WebAPI.Services;

using Microsoft.Extensions.Options;

public sealed class PendingApplicationCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<MerchantApplicationOptions> options,
    ILogger<PendingApplicationCleanupService> logger) : BackgroundService
{
    private readonly MerchantApplicationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(_options.PendingScanIntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IMerchantApplicationService service = scope.ServiceProvider.GetRequiredService<IMerchantApplicationService>();
            int updatedCount = await service.AutoRejectPendingApplicationsAsync(cancellationToken);

            if (updatedCount > 0)
            {
                logger.LogInformation("{Count} pending applications were automatically rejected.", updatedCount);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Pending application cleanup job failed.");
        }
    }
}
