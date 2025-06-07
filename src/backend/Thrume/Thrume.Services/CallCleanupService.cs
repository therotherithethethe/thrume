using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Thrume.Services;

public class CallCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CallCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Run cleanup every 5 minutes

    public CallCleanupService(IServiceProvider serviceProvider, ILogger<CallCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Call cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var callStateService = scope.ServiceProvider.GetRequiredService<ICallStateService>();
                
                await callStateService.CleanupExpiredCallsAsync();
                
                _logger.LogDebug("Call cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during call cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Call cleanup service stopped");
    }
}