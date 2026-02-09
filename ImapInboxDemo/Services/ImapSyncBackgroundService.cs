using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ImapInboxDemo.Services
{
    public class ImapSyncBackgroundService : BackgroundService
    {
        private readonly ILogger<ImapSyncBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Force sync trigger
        private bool _forceSyncRequested = false;

        // Sync interval
        private readonly TimeSpan _syncInterval = TimeSpan.FromSeconds(60);

        public ImapSyncBackgroundService(
            ILogger<ImapSyncBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // Called by API controller to trigger immediate sync
        public void TriggerImmediateSync()
        {
            _forceSyncRequested = true;
            _logger.LogInformation("Force sync requested.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IMAP Sync Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // If force sync was requested, run immediately
                    if (_forceSyncRequested)
                    {
                        _forceSyncRequested = false;
                        _logger.LogInformation("Running forced IMAP sync...");
                        await RunSync(stoppingToken);
                    }
                    else
                    {
                        // Normal scheduled sync
                        _logger.LogInformation("Running scheduled IMAP sync...");
                        await RunSync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("IMAP sync cancelled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in IMAP sync background loop.");
                }

                // Wait for next cycle unless forced sync happens
                var delayTask = Task.Delay(_syncInterval, stoppingToken);

                // If a force sync is requested during delay, break early
                while (!delayTask.IsCompleted)
                {
                    if (_forceSyncRequested)
                    {
                        _logger.LogInformation("Force sync interrupting wait period.");
                        break;
                    }

                    await Task.Delay(500, stoppingToken); // small polling interval
                }
            }

            _logger.LogInformation("IMAP Sync Background Service is stopping.");
        }

        private async Task RunSync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ImapSyncService>();

            await syncService.SyncInboxAsync(stoppingToken);
        }
    }
}