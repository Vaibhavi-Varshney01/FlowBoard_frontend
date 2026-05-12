using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Workspace.Services;
using FlowBoard.Workspace.Models;
using FlowBoard.Workspace.Repositories;
using FlowBoard.Workspace.DTOs;
using Models = FlowBoard.Workspace.Models;

namespace FlowBoard.Tests.Workspace
{
    [TestFixture]
    public class WorkspaceServiceTests
    {
        private IWorkspaceRepository _repo;
        private IWorkspaceService    _sut;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<IWorkspaceRepository>();
            _sut  = new WorkspaceServiceImpl(_repo);
        }

        // ── CreateWorkspace ───────────────────────────────────────────────────

        [Test]
        public async Task CreateWorkspace_WithValidRequest_ReturnsWorkspace()
        {
            var request = new CreateWorkspaceRequest
            {
                Name        = "My Team",
                Description = "Dev team workspace",
                OwnerId     = "user-1",
                Visibility  = "PRIVATE"
            };

            _repo.ExistsByNameAndOwnerId(request.Name, request.OwnerId).Returns(false);
            _repo.Save(Arg.Any<Models.Workspace>()).Returns(ci =>
            {
                var ws = ci.Arg<Models.Workspace>();
                ws.WorkspaceId = 1;
                return ws;
            });
            _repo.SaveMember(Arg.Any<WorkspaceMember>()).Returns(ci => ci.Arg<WorkspaceMember>());

            var result = await _sut.CreateWorkspace(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("My Team");
            result.OwnerId.Should().Be("user-1");
            result.Visibility.Should().Be("PRIVATE");
        }

        [Test]
        public async Task CreateWorkspace_WithDuplicateName_ThrowsException()
        {
            var request = new CreateWorkspaceRequest { Name = "Existing", OwnerId = "user-1" };
            _repo.ExistsByNameAndOwnerId(request.Name, request.OwnerId).Returns(true);

            var act = async () => await _sut.CreateWorkspace(request);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Test]
        public async Task GetById_WhenExists_ReturnsWorkspace()
        {
            var ws = new Models.Workspace { WorkspaceId = 1, Name = "Test WS", OwnerId = "user-1" };
            _repo.FindByWorkspaceId(1).Returns(ws);

            var result = await _sut.GetById(1);

            result.Should().NotBeNull();
            result.WorkspaceId.Should().Be(1);
        }

        [Test]
        public async Task GetById_WhenNotExists_ThrowsException()
        {
            _repo.FindByWorkspaceId(99).Returns((Models.Workspace?)null);

            var act = async () => await _sut.GetById(99);

            await act.Should().ThrowAsync<Exception>();
        }

        // ── GetByOwner ────────────────────────────────────────────────────────

        [Test]
        public async Task GetByOwner_ReturnsAllOwnerWorkspaces()
        {
            var workspaces = new List<Models.Workspace>
            {
                new() { WorkspaceId = 1, OwnerId = "user-1" },
                new() { WorkspaceId = 2, OwnerId = "user-1" }
            };
            _repo.FindByOwnerId("user-1").Returns(workspaces);

            var result = await _sut.GetByOwner("user-1");

            result.Should().HaveCount(2);
            result.All(w => w.OwnerId == "user-1").Should().BeTrue();
        }

        // ── UpdateWorkspace ───────────────────────────────────────────────────

        [Test]
        public async Task UpdateWorkspace_WithValidRequest_UpdatesAndReturns()
        {
            var existing = new Models.Workspace { WorkspaceId = 1, Name = "Old Name", OwnerId = "user-1" };
            var request  = new UpdateWorkspaceRequest { Name = "New Name", Visibility = "PUBLIC" };

            _repo.FindByWorkspaceId(1).Returns(existing);
            _repo.Update(Arg.Any<Models.Workspace>()).Returns(ci => ci.Arg<Models.Workspace>());

            var result = await _sut.UpdateWorkspace(1, request);

            result.Name.Should().Be("New Name");
            result.Visibility.Should().Be("PUBLIC");
        }

        // ── DeleteWorkspace ───────────────────────────────────────────────────

        [Test]
        public async Task DeleteWorkspace_ByOwner_DeletesSuccessfully()
        {
            var ws = new Models.Workspace { WorkspaceId = 1, OwnerId = "user-1" };
            _repo.FindByWorkspaceId(1).Returns(ws);
            _repo.Delete(1).Returns(Task.CompletedTask);

            await _sut.DeleteWorkspace(1, "user-1");

            await _repo.Received(1).Delete(1);
        }

        [Test]
        public async Task DeleteWorkspace_ByNonOwner_ThrowsException()
        {
            var ws = new Models.Workspace { WorkspaceId = 1, OwnerId = "user-1" };
            _repo.FindByWorkspaceId(1).Returns(ws);

            var act = async () => await _sut.DeleteWorkspace(1, "user-999");

            await act.Should().ThrowAsync<Exception>();
        }

        // ── AddMember ─────────────────────────────────────────────────────────

        [Test]
        public async Task AddMember_WithValidRequest_ReturnsMember()
        {
            var ws     = new Models.Workspace { WorkspaceId = 1, OwnerId = "user-1" };
            var req    = new AddMemberRequest { UserId = "user-2", Role = "MEMBER" };
            var member = new WorkspaceMember  { MemberId = 1, WorkspaceId = 1, UserId = "user-2", Role = "MEMBER" };

            _repo.FindByWorkspaceId(1).Returns(ws);
            _repo.IsMember(1, "user-2").Returns(false);
            _repo.SaveMember(Arg.Any<WorkspaceMember>()).Returns(member);

            var result = await _sut.AddMember(1, req);

            result.UserId.Should().Be("user-2");
            result.Role.Should().Be("MEMBER");
        }

        // ── GetMembers ────────────────────────────────────────────────────────

        [Test]
        public async Task GetMembers_ReturnsAllMembers()
        {
            var members = new List<WorkspaceMember>
            {
                new() { UserId = "user-1", Role = "ADMIN" },
                new() { UserId = "user-2", Role = "MEMBER" }
            };
            _repo.FindAllMembers(1).Returns(members);

            var result = await _sut.GetMembers(1);

            result.Should().HaveCount(2);
        }
    }
}