using Bookify.Application.Interfaces.Notification;

namespace Bookify.API.Services
{
    /// <summary>
    /// Background service that periodically cleans up notifications older than 30 days.
    /// Runs once every 24 hours.
    /// </summary>
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public NotificationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<NotificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                    var deleted = await notificationService.DeleteOlderThanAsync(30);

                    if (deleted > 0)
                        _logger.LogInformation(
                            "NotificationCleanupService: Deleted {Count} notifications older than 30 days.", deleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during notification cleanup.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
