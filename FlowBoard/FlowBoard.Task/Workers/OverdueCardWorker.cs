using FlowBoard.Card.Services;

namespace FlowBoard.Card.Workers
{
    /// <summary>
    /// Background worker that runs every hour and logs overdue cards to the console.
    /// In production this would push notifications or update a status field.
    /// Implements IHostedService via BackgroundService base class.
    /// </summary>
    public class OverdueCardWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueCardWorker> _logger;

        // Check interval — 1 hour in production, shorter for dev/test
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public OverdueCardWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OverdueCardWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[OverdueCardWorker] Background worker started. Interval: {Interval}.", CheckInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckOverdueCards(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OverdueCardWorker] Error while checking overdue cards.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("[OverdueCardWorker] Background worker stopping.");
        }

        private async Task CheckOverdueCards(CancellationToken cancellationToken)
        {
            // Use a new DI scope per cycle — ICardService is Scoped, not Singleton
            using var scope = _scopeFactory.CreateScope();
            var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();

            var overdueCards = await cardService.GetOverdueCards();

            if (!overdueCards.Any())
            {
                _logger.LogInformation("[OverdueCardWorker] No overdue cards found at {Time}.", DateTime.UtcNow);
                return;
            }

            _logger.LogWarning(
                "[OverdueCardWorker] {Count} overdue card(s) detected at {Time}:",
                overdueCards.Count, DateTime.UtcNow);

            foreach (var card in overdueCards)
            {
                _logger.LogWarning(
                    "  → CardId={CardId} | Title='{Title}' | DueDate={DueDate:d} | AssigneeId={AssigneeId}",
                    card.CardId, card.Title, card.DueDate, card.AssigneeId);
            }

            // TODO: Extend here to push email / in-app notifications via INotificationService
        }
    }
}