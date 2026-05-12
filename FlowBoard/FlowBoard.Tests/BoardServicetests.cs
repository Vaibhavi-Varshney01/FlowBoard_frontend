using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Board.Services;
using FlowBoard.Board.Models;
using FlowBoard.Board.Repositories;
using Models = FlowBoard.Board.Models;

namespace FlowBoard.Tests.Board
{
    [TestFixture]
    public class BoardServiceTests
    {
        private IBoardRepository   _repo;
        private IHttpClientFactory _httpClientFactory;
        private IBoardService      _sut;

        [SetUp]
        public void SetUp()
        {
            _repo              = Substitute.For<IBoardRepository>();
            _httpClientFactory = Substitute.For<IHttpClientFactory>();

            var mockHttpClient = new HttpClient(new FakeSuccessHttpMessageHandler());
            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(mockHttpClient);

            _sut = new BoardServiceImpl(_repo, _httpClientFactory);
        }

        // ── CreateBoard ───────────────────────────────────────────────────────

        [Test]
        public async Task CreateBoard_WithValidRequest_ReturnsBoard()
        {
            var request = new CreateBoardRequest(1, "Sprint Board", null, "#0079BF", "PRIVATE", "user-1");

            _repo.Save(Arg.Any<Models.Board>()).Returns(ci =>
            {
                var b = ci.Arg<Models.Board>();
                b.BoardId = 1;
                return b;
            });
            _repo.SaveMember(Arg.Any<BoardMember>()).Returns(ci => ci.Arg<BoardMember>());

            var result = await _sut.CreateBoard(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Sprint Board");
            result.WorkspaceId.Should().Be(1);
            result.IsClosed.Should().BeFalse();
        }

        // ── GetBoardById ──────────────────────────────────────────────────────

        [Test]
        public async Task GetBoardById_WhenExists_ReturnsBoard()
        {
            var board = new Models.Board { BoardId = 1, Name = "Sprint Board", WorkspaceId = 1 };
            _repo.FindByBoardId(1).Returns(board);

            var result = await _sut.GetBoardById(1);

            result.BoardId.Should().Be(1);
            result.Name.Should().Be("Sprint Board");
        }

        [Test]
        public async Task GetBoardById_WhenNotExists_ThrowsException()
        {
            _repo.FindByBoardId(99).Returns((Models.Board?)null);

            var act = async () => await _sut.GetBoardById(99);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetBoardsByWorkspace ──────────────────────────────────────────────

        [Test]
        public async Task GetBoardsByWorkspace_ReturnsAllBoards()
        {
            var boards = new List<Models.Board>
            {
                new() { BoardId = 1, WorkspaceId = 1, Name = "Board A" },
                new() { BoardId = 2, WorkspaceId = 1, Name = "Board B" }
            };
            _repo.FindByWorkspaceId(1).Returns(boards);

            var result = await _sut.GetBoardsByWorkspace(1);

            result.Should().HaveCount(2);
        }

        // ── CloseBoard ────────────────────────────────────────────────────────

        [Test]
        public async Task CloseBoard_ByOwner_SetIsClosedToTrue()
        {
            var board = new Models.Board { BoardId = 1, CreatedById = "user-1", IsClosed = false };
            _repo.FindByBoardId(1).Returns(board);
            _repo.FindMember(1, "user-1").Returns((BoardMember?)null);
            _repo.Update(Arg.Any<Models.Board>()).Returns(ci => ci.Arg<Models.Board>());

            var result = await _sut.CloseBoard(1, "user-1");

            result.IsClosed.Should().BeTrue();
        }

        [Test]
        public async Task CloseBoard_ByNonOwner_ThrowsException()
        {
            var board = new Models.Board { BoardId = 1, CreatedById = "user-1" };
            _repo.FindByBoardId(1).Returns(board);
            _repo.FindMember(1, "user-999").Returns((BoardMember?)null);

            var act = async () => await _sut.CloseBoard(1, "user-999");

            await act.Should().ThrowAsync<Exception>();
        }

        // ── DeleteBoard ───────────────────────────────────────────────────────

        [Test]
        public async Task DeleteBoard_ByOwner_DeletesSuccessfully()
        {
            var board = new Models.Board { BoardId = 1, CreatedById = "user-1" };
            _repo.FindByBoardId(1).Returns(board);
            _repo.Delete(1).Returns(Task.CompletedTask);

            await _sut.DeleteBoard(1, "user-1");

            await _repo.Received(1).Delete(1);
        }

        // ── AddMember ─────────────────────────────────────────────────────────

        [Test]
        public async Task AddMember_ReturnsAddedMember()
        {
            var board  = new Models.Board { BoardId = 1, CreatedById = "user-1" };
            var member = new BoardMember  { BoardId = 1, UserId = "user-2", Role = "MEMBER" };
            var req    = new AddMemberRequest("user-2", "MEMBER");

            _repo.FindByBoardId(1).Returns(board);
            _repo.FindMember(1, "user-1").Returns((BoardMember?)null);
            _repo.IsMember(1, "user-2").Returns(false);
            _repo.SaveMember(Arg.Any<BoardMember>()).Returns(member);

            var result = await _sut.AddMember(1, req, "user-1");

            result.UserId.Should().Be("user-2");
            result.Role.Should().Be("MEMBER");
        }

        // ── UpdateMemberRole ──────────────────────────────────────────────────

        [Test]
        public async Task UpdateMemberRole_ChangesRole()
        {
            var board  = new Models.Board { BoardId = 1, CreatedById = "user-1" };
            var member = new BoardMember  { BoardId = 1, UserId = "user-2", Role = "MEMBER" };

            _repo.FindByBoardId(1).Returns(board);
            _repo.FindMember(1, "user-1").Returns((BoardMember?)null);
            _repo.FindMember(1, "user-2").Returns(member);
            _repo.UpdateMember(Arg.Any<BoardMember>()).Returns(ci => ci.Arg<BoardMember>());

            var result = await _sut.UpdateMemberRole(1, "user-2", "ADMIN", "user-1");

            result.Role.Should().Be("ADMIN");
        }
    }

    /// <summary>Fake HTTP handler that always returns 200 OK for SAGA calls.</summary>
    internal class FakeSuccessHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken  cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}