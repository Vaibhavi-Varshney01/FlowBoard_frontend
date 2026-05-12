using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Card.Services;
using FlowBoard.Card.Models;
using FlowBoard.Card.Repositories;
using Models = FlowBoard.Card.Models;

namespace FlowBoard.Tests.Card
{
    [TestFixture]
    public class CardServiceTests
    {
        private ICardRepository _repo;
        private ICardService    _sut;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<ICardRepository>();
            _sut  = new CardServiceImpl(_repo);
        }

        // ── CreateCard ────────────────────────────────────────────────────────

        [Test]
        public async Task CreateCard_WithValidRequest_ReturnsCard()
        {
            var request = new CreateCardRequest(
                ListId: 1, BoardId: 1, Title: "Fix Login Bug",
                Description: null, Priority: Priority.HIGH,
                Status: CardStatus.TO_DO, DueDate: null,
                StartDate: null, AssigneeId: null,
                CreatedById: "user-1", CoverColor: null);

            // Simulate 2 existing cards in list
            _repo.FindByListIdOrderByPosition(1).Returns(new List<Models.Card>
            {
                new() { CardId = 1, Position = 1 },
                new() { CardId = 2, Position = 2 }
            });
            _repo.Save(Arg.Any<Models.Card>()).Returns(ci =>
            {
                var c = ci.Arg<Models.Card>();
                c.CardId = 3;
                return c;
            });
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.CreateCard(request);

            result.Should().NotBeNull();
            result.Title.Should().Be("Fix Login Bug");
            result.Priority.Should().Be(Priority.HIGH);
            result.Status.Should().Be(CardStatus.TO_DO);
            result.IsArchived.Should().BeFalse();
        }

        [Test]
        public async Task CreateCard_AssignsCorrectPosition()
        {
            var request = new CreateCardRequest(1, 1, "Task", null, Priority.LOW,
                CardStatus.TO_DO, null, null, null, "user-1", null);

            // 4 existing cards → new card gets position 5
            _repo.FindByListIdOrderByPosition(1).Returns(new List<Models.Card>
            {
                new() { CardId = 1, Position = 1 },
                new() { CardId = 2, Position = 2 },
                new() { CardId = 3, Position = 3 },
                new() { CardId = 4, Position = 4 }
            });
            _repo.Save(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.CreateCard(request);

            result.Position.Should().Be(5);
        }

        // ── GetCardById ───────────────────────────────────────────────────────

        [Test]
        public async Task GetCardById_WhenExists_ReturnsCard()
        {
            var card = new Models.Card { CardId = 1, Title = "Fix Bug", ListId = 1, BoardId = 1 };
            _repo.FindByCardId(1).Returns(card);

            var result = await _sut.GetCardById(1);

            result.CardId.Should().Be(1);
            result.Title.Should().Be("Fix Bug");
        }

        [Test]
        public async Task GetCardById_WhenNotExists_ThrowsException()
        {
            _repo.FindByCardId(99).Returns((Models.Card?)null);

            var act = async () => await _sut.GetCardById(99);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetCardsByList ────────────────────────────────────────────────────

        [Test]
        public async Task GetCardsByList_ReturnsOrderedCards()
        {
            var cards = new List<Models.Card>
            {
                new() { CardId = 1, ListId = 1, Position = 1, Title = "Task 1" },
                new() { CardId = 2, ListId = 1, Position = 2, Title = "Task 2" }
            };
            _repo.FindByListIdOrderByPosition(1).Returns(cards);

            var result = await _sut.GetCardsByList(1);

            result.Should().HaveCount(2);
            result.First().Position.Should().Be(1);
        }

        // ── MoveCard ──────────────────────────────────────────────────────────

        [Test]
        public async Task MoveCard_ChangesListAndBoard()
        {
            var card    = new Models.Card { CardId = 1, ListId = 1, BoardId = 1, Position = 1 };
            var request = new MoveCardRequest(TargetListId: 2, TargetBoardId: 1, TargetPosition: 1);

            _repo.FindByCardId(1).Returns(card);
            // Origin list siblings (excluding the card being moved)
            _repo.FindByListIdOrderByPosition(1).Returns(new List<Models.Card> { card });
            // Target list is empty
            _repo.FindByListIdOrderByPosition(2).Returns(new List<Models.Card>());
            _repo.Update(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.UpdateRangePositions(Arg.Any<List<Models.Card>>()).Returns(Task.CompletedTask);
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.MoveCard(1, request, "user-1");

            result.ListId.Should().Be(2);
            result.Position.Should().Be(1);
        }

        // ── ArchiveCard ───────────────────────────────────────────────────────

        [Test]
        public async Task ArchiveCard_SetsIsArchivedTrue()
        {
            var card = new Models.Card { CardId = 1, IsArchived = false, ListId = 1 };
            _repo.FindByCardId(1).Returns(card);
            _repo.Update(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.FindByListIdOrderByPosition(1).Returns(new List<Models.Card>());
            _repo.UpdateRangePositions(Arg.Any<List<Models.Card>>()).Returns(Task.CompletedTask);
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.ArchiveCard(1, "user-1");

            result.IsArchived.Should().BeTrue();
        }

        // ── UnarchiveCard ─────────────────────────────────────────────────────

        [Test]
        public async Task UnarchiveCard_SetsIsArchivedFalse()
        {
            var card = new Models.Card { CardId = 1, IsArchived = true, ListId = 1 };
            _repo.FindByCardId(1).Returns(card);
            // Siblings used to determine next position
            _repo.FindByListIdOrderByPosition(1).Returns(new List<Models.Card>());
            _repo.Update(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.UnarchiveCard(1, "user-1");

            result.IsArchived.Should().BeFalse();
        }

        // ── SetAssignee ───────────────────────────────────────────────────────

        [Test]
        public async Task SetAssignee_UpdatesAssigneeId()
        {
            var card    = new Models.Card { CardId = 1, AssigneeId = null, IsArchived = false };
            var request = new SetAssigneeRequest("user-5");

            _repo.FindByCardId(1).Returns(card);
            _repo.Update(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.SetAssignee(1, request, "user-1");

            result.AssigneeId.Should().Be("user-5");
        }

        // ── SetPriority ───────────────────────────────────────────────────────

        [Test]
        public async Task SetPriority_UpdatesPriority()
        {
            var card    = new Models.Card { CardId = 1, Priority = Priority.LOW, IsArchived = false };
            var request = new SetPriorityRequest(Priority.CRITICAL);

            _repo.FindByCardId(1).Returns(card);
            _repo.Update(Arg.Any<Models.Card>()).Returns(ci => ci.Arg<Models.Card>());
            _repo.LogActivity(Arg.Any<CardActivity>()).Returns(Task.CompletedTask);

            var result = await _sut.SetPriority(1, request, "user-1");

            result.Priority.Should().Be(Priority.CRITICAL);
        }

        // ── GetOverdueCards ───────────────────────────────────────────────────

        [Test]
        public async Task GetOverdueCards_ReturnsOnlyOverdueNonDoneCards()
        {
            var overdue = new List<Models.Card>
            {
                new() { CardId = 1, DueDate = DateTime.UtcNow.AddDays(-2), Status = CardStatus.IN_PROGRESS },
                new() { CardId = 2, DueDate = DateTime.UtcNow.AddDays(-1), Status = CardStatus.TO_DO }
            };
            _repo.FindByDueDateBefore(Arg.Any<DateTime>()).Returns(overdue);

            var result = await _sut.GetOverdueCards();

            result.Should().HaveCount(2);
            result.All(c => c.Status != CardStatus.DONE).Should().BeTrue();
        }

        // ── FilterCards ───────────────────────────────────────────────────────

        [Test]
        public async Task FilterCards_ByPriority_ReturnsCriticalCards()
        {
            var cards  = new List<Models.Card> { new() { CardId = 1, Priority = Priority.CRITICAL } };
            var filter = new CardFilterRequest(null, null, Priority.CRITICAL, null);
            _repo.FindByPriority(Priority.CRITICAL).Returns(cards);

            var result = await _sut.FilterCards(filter);

            result.Should().HaveCount(1);
            result.First().Priority.Should().Be(Priority.CRITICAL);
        }
    }
}