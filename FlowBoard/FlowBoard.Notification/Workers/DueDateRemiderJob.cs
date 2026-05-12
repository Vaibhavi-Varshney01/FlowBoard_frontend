using FlowBoard.Notification.Events;
using MassTransit;
using Quartz;

namespace FlowBoard.Notification.Workers
{
    /// <summary>
    /// Scheduled job that fires at two points before a card's due date:
    ///   • 1 day  before → publishes DueDateReminderEvent with Horizon = "1 day"
    ///   • 1 hour before → publishes DueDateReminderEvent with Horizon = "1 hour"
    ///
    /// The job is triggered by Quartz.NET via a CronTrigger (e.g. every 30 min).
    /// The Task-Service is responsible for calling into this scheduler when a card
    /// due date is set or changed (or you can scan all cards here via HTTP).
    /// </summary>
    [DisallowConcurrentExecution]
    public class DueDateReminderJob : IJob
    {
        private readonly IPublishEndpoint _bus;
        private readonly ILogger<DueDateReminderJob> _logger;

        // In a real implementation this would call the Task-Service gRPC/HTTP endpoint
        // or share a read-only DB view.  For the MVP we inject a dummy card provider.
        private readonly IDueDateCardProvider _cardProvider;

        public DueDateReminderJob(
            IPublishEndpoint bus,
            ILogger<DueDateReminderJob> logger,
            IDueDateCardProvider cardProvider)
        {
            _bus          = bus;
            _logger       = logger;
            _cardProvider = cardProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("[DueDateReminderJob] Running at {Time}", DateTime.UtcNow);

            var now       = DateTime.UtcNow;
            var oneDay    = now.AddHours(24);
            var oneHour   = now.AddHours(1);
            var tolerance = TimeSpan.FromMinutes(15);  // window to fire "~1 day" / "~1 hour"

            var cards = await _cardProvider.GetCardsWithDueDateAsync();

            foreach (var card in cards)
            {
                if (!card.DueAt.HasValue || string.IsNullOrEmpty(card.AssigneeId))
                    continue;

                var due = card.DueAt.Value;

                if (Math.Abs((due - oneDay).TotalMinutes) <= tolerance.TotalMinutes)
                {
                    await Publish(card, "1 day");
                }
                else if (Math.Abs((due - oneHour).TotalMinutes) <= tolerance.TotalMinutes)
                {
                    await Publish(card, "1 hour");
                }
            }
        }

        private async Task Publish(DueDateCard card, string horizon)
        {
            _logger.LogInformation(
                "[DueDateReminderJob] Publishing reminder for Card {CardId} ({Horizon})",
                card.CardId, horizon);

            await _bus.Publish(new DueDateReminderEvent
            {
                CardId    = card.CardId,
                CardTitle = card.Title,
                AssigneeId = card.AssigneeId!,
                DueAt     = card.DueAt!.Value,
                Horizon   = horizon,
                OccurredAt = DateTime.UtcNow
            });
        }
    }

    // ── Card DTO used by the job ──────────────────────────────────────────────

    public class DueDateCard
    {
        public int     CardId     { get; set; }
        public string  Title      { get; set; } = string.Empty;
        public string? AssigneeId { get; set; }
        public DateTime? DueAt   { get; set; }
    }

    // ── Provider interface (implement by calling Task-Service API) ────────────

    public interface IDueDateCardProvider
    {
        Task<List<DueDateCard>> GetCardsWithDueDateAsync();
    }

    /// <summary>
    /// Stub implementation — replace with real HTTP call to Task-Service.
    /// </summary>
    public class HttpDueDateCardProvider : IDueDateCardProvider
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpDueDateCardProvider> _logger;

        public HttpDueDateCardProvider(HttpClient http, ILogger<HttpDueDateCardProvider> logger)
        {
            _http   = http;
            _logger = logger;
        }

        public async Task<List<DueDateCard>> GetCardsWithDueDateAsync()
        {
            try
            {
                var cards = await _http.GetFromJsonAsync<List<DueDateCard>>("api/cards/due-soon");
                return cards ?? new List<DueDateCard>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HttpDueDateCardProvider] Failed to fetch cards from Task-Service");
                return new List<DueDateCard>();
            }
        }
    }
}