using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Notification.Services;
using FlowBoard.Notification.Models;
using FlowBoard.Notification.Repositories;
using FlowBoard.Notification.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using Models = FlowBoard.Notification.Models;

namespace FlowBoard.Tests.Notification
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private INotificationRepository      _repo;
        private IHubContext<NotificationHub> _hubContext;
        private IPublishEndpoint             _bus;
        private ISendGridClient              _emailSender;
        private IConfiguration               _config;
        private ILogger<NotificationServiceImpl> _logger;
        private INotificationService         _sut;

        [SetUp]
        public void SetUp()
        {
            _repo        = Substitute.For<INotificationRepository>();
            _hubContext  = Substitute.For<IHubContext<NotificationHub>>();
            _bus         = Substitute.For<IPublishEndpoint>();
            _emailSender = Substitute.For<ISendGridClient>();
            _config      = Substitute.For<IConfiguration>();
            _logger      = Substitute.For<ILogger<NotificationServiceImpl>>();

            // Mock SignalR clients chain — service uses hub.Clients.Group(...)
            var mockClients = Substitute.For<IHubClients>();
            var mockClient  = Substitute.For<IClientProxy>();
            mockClients.Group(Arg.Any<string>()).Returns(mockClient);
            _hubContext.Clients.Returns(mockClients);

            // Default unread count
            _repo.CountByRecipientIdAndIsRead(Arg.Any<string>(), false).Returns(0);

            _sut = new NotificationServiceImpl(
                _repo, _bus, _hubContext, _emailSender, _config, _logger);
        }

        // ── Send ──────────────────────────────────────────────────────────────

        [Test]
        public async Task Send_WithValidRequest_SavesAndReturnsNotification()
        {
            var request = new SendNotificationRequest(
                RecipientId: "user-2",
                ActorId:     "user-1",
                Type:        NotificationType.ASSIGNMENT,
                Message:     "You were assigned to Fix Login Bug",
                Title:       "New Assignment",
                RelatedId:   1,
                RelatedType: "Card"
            );

            var saved = new Models.Notification
            {
                NotificationId = 1,
                RecipientId    = "user-2",
                ActorId        = "user-1",
                Type           = NotificationType.ASSIGNMENT,
                Message        = "You were assigned to Fix Login Bug",
                IsRead         = false
            };

            _repo.Save(Arg.Any<Models.Notification>()).Returns(saved);

            var result = await _sut.Send(request);

            result.Should().NotBeNull();
            result.RecipientId.Should().Be("user-2");
            result.Type.Should().Be(NotificationType.ASSIGNMENT);
            result.IsRead.Should().BeFalse();
        }

        // ── SendBulk ──────────────────────────────────────────────────────────

        [Test]
        public async Task SendBulk_ToMultipleRecipients_PublishesEvent()
        {
            var request = new SendBulkNotificationRequest(
                RecipientIds: new List<string> { "user-1", "user-2", "user-3" },
                ActorId:      "admin-1",
                Type:         NotificationType.MENTION,
                Message:      "Platform maintenance tonight",
                Title:        "System Alert"
            );

            await _sut.SendBulk(request);

            // SendBulk publishes via MassTransit — verify the generic Publish<T> was called once
            await _bus.Received(1).Publish(Arg.Any<FlowBoard.Notification.Events.BulkNotificationEvent>(), Arg.Any<CancellationToken>());
        }

        // ── MarkAsRead ────────────────────────────────────────────────────────

        [Test]
        public async Task MarkAsRead_SetsIsReadTrue()
        {
            var notif = new Models.Notification { NotificationId = 1, RecipientId = "user-1", IsRead = false };
            _repo.FindByNotificationId(1).Returns(notif);
            _repo.Update(Arg.Any<Models.Notification>()).Returns(ci => ci.Arg<Models.Notification>());

            await _sut.MarkAsRead(1);

            await _repo.Received(1).Update(Arg.Is<Models.Notification>(n => n.IsRead == true));
        }

        // ── MarkAllRead ───────────────────────────────────────────────────────

        [Test]
        public async Task MarkAllRead_MarksAllForRecipient()
        {
            var notifs = new List<Models.Notification>
            {
                new() { NotificationId = 1, RecipientId = "user-1", IsRead = false },
                new() { NotificationId = 2, RecipientId = "user-1", IsRead = false }
            };

            _repo.FindByRecipientIdAndIsRead("user-1", false).Returns(notifs);
            _repo.Update(Arg.Any<Models.Notification>()).Returns(ci => ci.Arg<Models.Notification>());

            await _sut.MarkAllRead("user-1");

            await _repo.Received(2).Update(Arg.Is<Models.Notification>(n => n.IsRead == true));
        }

        // ── GetByRecipient ────────────────────────────────────────────────────

        [Test]
        public async Task GetByRecipient_ReturnsAllForUser()
        {
            var notifs = new List<Models.Notification>
            {
                new() { NotificationId = 1, RecipientId = "user-1", Type = NotificationType.ASSIGNMENT },
                new() { NotificationId = 2, RecipientId = "user-1", Type = NotificationType.COMMENT }
            };
            _repo.FindByRecipientId("user-1").Returns(notifs);

            var result = await _sut.GetByRecipient("user-1");

            result.Should().HaveCount(2);
            result.All(n => n.RecipientId == "user-1").Should().BeTrue();
        }

        // ── GetUnreadCount ────────────────────────────────────────────────────

        [Test]
        public async Task GetUnreadCount_ReturnsCorrectCount()
        {
            _repo.CountByRecipientIdAndIsRead("user-1", false).Returns(3);

            var result = await _sut.GetUnreadCount("user-1");

            result.Should().Be(3);
        }

        [Test]
        public async Task GetUnreadCount_WhenAllRead_ReturnsZero()
        {
            _repo.CountByRecipientIdAndIsRead("user-1", false).Returns(0);

            var result = await _sut.GetUnreadCount("user-1");

            result.Should().Be(0);
        }

        // ── DeleteRead ────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteRead_RemovesAllReadNotifications()
        {
            _repo.DeleteByRecipientIdAndIsRead("user-1", true).Returns(Task.CompletedTask);

            await _sut.DeleteRead("user-1");

            await _repo.Received(1).DeleteByRecipientIdAndIsRead("user-1", true);
        }

        // ── Notification Type Validation ──────────────────────────────────────

        [Test]
        [TestCase(NotificationType.ASSIGNMENT)]
        [TestCase(NotificationType.MENTION)]
        [TestCase(NotificationType.DUE_DATE)]
        [TestCase(NotificationType.COMMENT)]
        [TestCase(NotificationType.MOVE)]
        public async Task Send_AllNotificationTypes_AreSupported(NotificationType type)
        {
            var request = new SendNotificationRequest("user-2", "user-1", type, "Test message", "Test");
            var notif   = new Models.Notification { NotificationId = 1, RecipientId = "user-2", Type = type };
            _repo.Save(Arg.Any<Models.Notification>()).Returns(notif);

            var result = await _sut.Send(request);

            result.Type.Should().Be(type);
        }
    }
}