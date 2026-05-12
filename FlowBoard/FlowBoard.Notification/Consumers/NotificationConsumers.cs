using FlowBoard.Notification.Events;
using FlowBoard.Notification.Models;
using FlowBoard.Notification.Services;
using MassTransit;

namespace FlowBoard.Notification.Consumers
{
    // ── Card Assigned ─────────────────────────────────────────────────────────

    public class CardAssignedConsumer : IConsumer<CardAssignedEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<CardAssignedConsumer> _logger;

        public CardAssignedConsumer(INotificationService service, ILogger<CardAssignedConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<CardAssignedEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] CardAssigned: Card {CardId} -> {Assignee}", ev.CardId, ev.AssigneeId);

            await _service.Send(new SendNotificationRequest(
                RecipientId: ev.AssigneeId,
                ActorId:     ev.ActorId,
                Type:        NotificationType.ASSIGNMENT,
                Title:       "Card Assigned to You",
                Message:     $"You have been assigned to card: \"{ev.CardTitle}\".",
                RelatedId:   ev.CardId,
                RelatedType: "Card"
            ));
        }
    }

    // ── @Mention ──────────────────────────────────────────────────────────────

    public class MentionConsumer : IConsumer<MentionEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<MentionConsumer> _logger;

        public MentionConsumer(INotificationService service, ILogger<MentionConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<MentionEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] Mention: {Actor} mentioned {Recipient} in Comment {CommentId}", ev.ActorId, ev.MentionedId, ev.CommentId);

            await _service.Send(new SendNotificationRequest(
                RecipientId: ev.MentionedId,
                ActorId:     ev.ActorId,
                Type:        NotificationType.MENTION,
                Title:       "You Were Mentioned",
                Message:     $"You were mentioned in a comment: \"{ev.Excerpt}\".",
                RelatedId:   ev.CardId,
                RelatedType: "Card"
            ));
        }
    }

    // ── Comment Reply ─────────────────────────────────────────────────────────

    public class CommentReplyConsumer : IConsumer<CommentReplyEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<CommentReplyConsumer> _logger;

        public CommentReplyConsumer(INotificationService service, ILogger<CommentReplyConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<CommentReplyEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] CommentReply: {Actor} replied to {ParentAuthor}", ev.ActorId, ev.ParentAuthorId);

            await _service.Send(new SendNotificationRequest(
                RecipientId: ev.ParentAuthorId,
                ActorId:     ev.ActorId,
                Type:        NotificationType.COMMENT,
                Title:       "New Reply to Your Comment",
                Message:     $"Someone replied to your comment: \"{ev.Excerpt}\".",
                RelatedId:   ev.CardId,
                RelatedType: "Card"
            ));
        }
    }

    // ── Card Moved to Done ────────────────────────────────────────────────────

    public class CardMovedToDoneConsumer : IConsumer<CardMovedToDoneEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<CardMovedToDoneConsumer> _logger;

        public CardMovedToDoneConsumer(INotificationService service, ILogger<CardMovedToDoneConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<CardMovedToDoneEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] CardMovedToDone: Card {CardId} by {Actor}", ev.CardId, ev.ActorId);

            await _service.Send(new SendNotificationRequest(
                RecipientId: ev.RecipientId,
                ActorId:     ev.ActorId,
                Type:        NotificationType.MOVE,
                Title:       "Card Moved to Done",
                Message:     $"Card \"{ev.CardTitle}\" has been moved to Done.",
                RelatedId:   ev.CardId,
                RelatedType: "Card"
            ));
        }
    }

    // ── Due Date Reminder ─────────────────────────────────────────────────────

    public class DueDateReminderConsumer : IConsumer<DueDateReminderEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<DueDateReminderConsumer> _logger;

        public DueDateReminderConsumer(INotificationService service, ILogger<DueDateReminderConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<DueDateReminderEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] DueDateReminder: Card {CardId} due in {Horizon}", ev.CardId, ev.Horizon);

            await _service.Send(new SendNotificationRequest(
                RecipientId: ev.AssigneeId,
                ActorId:     "system",
                Type:        NotificationType.DUE_DATE,
                Title:       $"Card Due in {ev.Horizon}",
                Message:     $"Card \"{ev.CardTitle}\" is due in {ev.Horizon}. Don't miss it!",
                RelatedId:   ev.CardId,
                RelatedType: "Card"
            ));
        }
    }

    // ── Bulk Notification (admin fan-out) ─────────────────────────────────────

    public class BulkNotificationConsumer : IConsumer<BulkNotificationEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<BulkNotificationConsumer> _logger;

        public BulkNotificationConsumer(INotificationService service, ILogger<BulkNotificationConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<BulkNotificationEvent> context)
        {
            var ev = context.Message;
            _logger.LogInformation("[Consumer] BulkNotification: {Count} recipients", ev.RecipientIds.Count);

            var tasks = ev.RecipientIds.Select(recipientId =>
                _service.Send(new SendNotificationRequest(
                    RecipientId: recipientId,
                    ActorId:     ev.ActorId,
                    Type:        ev.Type,
                    Message:     ev.Message,
                    Title:       ev.Title,
                    RelatedId:   ev.RelatedId,
                    RelatedType: ev.RelatedType
                ))
            );

            await Task.WhenAll(tasks);
        }
    }
}