using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.List.Services;
using FlowBoard.List.Models;
using FlowBoard.List.Repositories;

namespace FlowBoard.Tests.Column
{
    [TestFixture]
    public class ColumnServiceTests
    {
        private IListRepository _repo;
        private IListService    _sut;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<IListRepository>();
            _sut  = new ListServiceImpl(_repo);
        }

        // ── CreateList ────────────────────────────────────────────────────────

        [Test]
        public async Task CreateList_WithValidRequest_ReturnsTaskList()
        {
            var request = new CreateListRequest(1, "To Do", "#FFFFFF");
            _repo.FindMaxPositionByBoardId(1).Returns(0);
            _repo.Save(Arg.Any<TaskList>()).Returns(ci =>
            {
                var tl = ci.Arg<TaskList>();
                tl.ListId = 1;
                return tl;
            });

            var result = await _sut.CreateList(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("To Do");
            result.BoardId.Should().Be(1);
            result.IsArchived.Should().BeFalse();
        }

        [Test]
        public async Task CreateList_AssignsNextPosition()
        {
            var request = new CreateListRequest(1, "In Progress", null);
            _repo.FindMaxPositionByBoardId(1).Returns(2);
            _repo.Save(Arg.Any<TaskList>()).Returns(ci => ci.Arg<TaskList>());

            var result = await _sut.CreateList(request);

            result.Position.Should().Be(3);
        }

        // ── GetListById ───────────────────────────────────────────────────────

        [Test]
        public async Task GetListById_WhenExists_ReturnsList()
        {
            var list = new TaskList { ListId = 1, BoardId = 1, Name = "To Do", Position = 1 };
            _repo.FindByListId(1).Returns(list);

            var result = await _sut.GetListById(1);

            result.ListId.Should().Be(1);
            result.Name.Should().Be("To Do");
        }

        [Test]
        public async Task GetListById_WhenNotExists_ThrowsException()
        {
            _repo.FindByListId(99).Returns((TaskList?)null);

            var act = async () => await _sut.GetListById(99);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetListsByBoard ───────────────────────────────────────────────────

        [Test]
        public async Task GetListsByBoard_ReturnsOrderedLists()
        {
            var lists = new List<TaskList>
            {
                new() { ListId = 1, BoardId = 1, Name = "To Do",      Position = 1 },
                new() { ListId = 2, BoardId = 1, Name = "In Progress", Position = 2 },
                new() { ListId = 3, BoardId = 1, Name = "Done",        Position = 3 }
            };
            _repo.FindByBoardIdOrderByPosition(1).Returns(lists);

            var result = await _sut.GetListsByBoard(1);

            result.Should().HaveCount(3);
            result.First().Name.Should().Be("To Do");
            result.Last().Name.Should().Be("Done");
        }

        // ── ReorderLists ──────────────────────────────────────────────────────

        [Test]
        public async Task ReorderLists_UpdatesPositions()
        {
            var lists = new List<TaskList>
            {
                new() { ListId = 1, Position = 1 },
                new() { ListId = 2, Position = 2 }
            };
            var request = new ReorderListsRequest(new List<ListPositionEntry>
            {
                new(1, 2),
                new(2, 1)
            });

            _repo.FindByBoardIdOrderByPosition(1).Returns(lists);
            _repo.UpdateRangePositions(Arg.Any<List<TaskList>>()).Returns(Task.CompletedTask);

            var result = await _sut.ReorderLists(1, request);

            result.Should().HaveCount(2);
            await _repo.Received(1).UpdateRangePositions(Arg.Any<List<TaskList>>());
        }

        // ── ArchiveList ───────────────────────────────────────────────────────

        [Test]
        public async Task ArchiveList_SetsIsArchivedTrue()
        {
            var list = new TaskList { ListId = 1, BoardId = 1, IsArchived = false };
            _repo.FindByListId(1).Returns(list);
            _repo.Update(Arg.Any<TaskList>()).Returns(ci => ci.Arg<TaskList>());
            _repo.FindByBoardIdOrderByPosition(1).Returns(new List<TaskList>());
            _repo.UpdateRangePositions(Arg.Any<List<TaskList>>()).Returns(Task.CompletedTask);

            var result = await _sut.ArchiveList(1);

            result.IsArchived.Should().BeTrue();
        }

        // ── UnarchiveList ─────────────────────────────────────────────────────

        [Test]
        public async Task UnarchiveList_SetsIsArchivedFalse()
        {
            var list = new TaskList { ListId = 1, BoardId = 1, IsArchived = true };
            _repo.FindByListId(1).Returns(list);
            _repo.FindMaxPositionByBoardId(1).Returns(2);
            _repo.Update(Arg.Any<TaskList>()).Returns(ci => ci.Arg<TaskList>());

            var result = await _sut.UnarchiveList(1);

            result.IsArchived.Should().BeFalse();
        }

        // ── MoveList ──────────────────────────────────────────────────────────

        [Test]
        public async Task MoveList_ChangesBoard()
        {
            var list    = new TaskList { ListId = 1, BoardId = 1 };
            var request = new MoveListRequest(2);

            _repo.FindByListId(1).Returns(list);
            _repo.FindMaxPositionByBoardId(2).Returns(3);
            _repo.Update(Arg.Any<TaskList>()).Returns(ci => ci.Arg<TaskList>());
            _repo.FindByBoardIdOrderByPosition(1).Returns(new List<TaskList>());
            _repo.UpdateRangePositions(Arg.Any<List<TaskList>>()).Returns(Task.CompletedTask);

            var result = await _sut.MoveList(1, request);

            result.BoardId.Should().Be(2);
        }

        // ── DeleteList ────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteList_CallsRepoDelete()
        {
            var list = new TaskList { ListId = 1, BoardId = 1 };
            _repo.FindByListId(1).Returns(list);
            _repo.FindByBoardIdOrderByPosition(1).Returns(new List<TaskList>());
            _repo.DeleteByListId(1).Returns(Task.CompletedTask);
            _repo.UpdateRangePositions(Arg.Any<List<TaskList>>()).Returns(Task.CompletedTask);

            await _sut.DeleteList(1);

            await _repo.Received(1).DeleteByListId(1);
        }
    }
}
