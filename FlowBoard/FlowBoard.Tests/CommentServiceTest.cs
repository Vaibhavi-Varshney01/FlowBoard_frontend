using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Comment.Services;
using FlowBoard.Comment.Models;
using FlowBoard.Comment.Repositories;
using FlowBoard.Comment.Storage;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Models = FlowBoard.Comment.Models;

namespace FlowBoard.Tests.Comment
{
    [TestFixture]
    public class CommentServiceTests
    {
        private ICommentRepository _repo;
        private IS3Service         _s3;
        private IPublishEndpoint   _bus;
        private ICommentService    _sut;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<ICommentRepository>();
            _s3   = Substitute.For<IS3Service>();
            _bus  = Substitute.For<IPublishEndpoint>();
            _sut  = new CommentServiceImpl(_repo, _s3, _bus);
        }

        // ── AddComment ────────────────────────────────────────────────────────

        [Test]
        public async Task AddComment_TopLevel_ReturnsComment()
        {
            var request = new AddCommentRequest(CardId: 1, AuthorId: "user-1",
                Content: "Great work!", ParentCommentId: null);

            _repo.Save(Arg.Any<Models.Comment>()).Returns(ci =>
            {
                var c = ci.Arg<Models.Comment>();
                c.CommentId = 1;
                return c;
            });

            var result = await _sut.AddComment(request);

            result.Should().NotBeNull();
            result.CardId.Should().Be(1);
            result.Content.Should().Be("Great work!");
            result.ParentCommentId.Should().BeNull();
            result.IsDeleted.Should().BeFalse();
        }

        [Test]
        public async Task AddComment_Reply_SetsParentCommentId()
        {
            var parent  = new Models.Comment { CommentId = 5, CardId = 1, IsDeleted = false };
            var request = new AddCommentRequest(CardId: 1, AuthorId: "user-2",
                Content: "I agree!", ParentCommentId: 5);

            _repo.FindByCommentId(5).Returns(parent);
            _repo.Save(Arg.Any<Models.Comment>()).Returns(ci => ci.Arg<Models.Comment>());

            var result = await _sut.AddComment(request);

            result.ParentCommentId.Should().Be(5);
        }

        // ── GetByCard ─────────────────────────────────────────────────────────

        [Test]
        public async Task GetByCard_ReturnsTopLevelComments()
        {
            var comments = new List<Models.Comment>
            {
                new() { CommentId = 1, CardId = 1, Content = "First comment",  IsDeleted = false },
                new() { CommentId = 2, CardId = 1, Content = "Second comment", IsDeleted = false }
            };
            _repo.FindByCardId(1).Returns(comments);

            var result = await _sut.GetByCard(1);

            result.Should().HaveCount(2);
            result.All(c => c.IsDeleted == false).Should().BeTrue();
        }

        // ── GetReplies ────────────────────────────────────────────────────────

        [Test]
        public async Task GetReplies_ReturnsChildComments()
        {
            var parent  = new Models.Comment { CommentId = 1, CardId = 1, IsDeleted = false };
            var replies = new List<Models.Comment>
            {
                new() { CommentId = 10, ParentCommentId = 1, Content = "Reply 1" },
                new() { CommentId = 11, ParentCommentId = 1, Content = "Reply 2" }
            };

            _repo.FindByCommentId(1).Returns(parent);
            _repo.FindByParentCommentId(1).Returns(replies);

            var result = await _sut.GetReplies(1);

            result.Should().HaveCount(2);
            result.All(c => c.ParentCommentId == 1).Should().BeTrue();
        }

        // ── UpdateComment ─────────────────────────────────────────────────────

        [Test]
        public async Task UpdateComment_ByAuthor_UpdatesContent()
        {
            var comment = new Models.Comment
                { CommentId = 1, AuthorId = "user-1", Content = "Old", IsDeleted = false };
            var request = new UpdateCommentRequest("Updated content");

            _repo.FindByCommentId(1).Returns(comment);
            _repo.Update(Arg.Any<Models.Comment>()).Returns(ci => ci.Arg<Models.Comment>());

            var result = await _sut.UpdateComment(1, request, "user-1");

            result.Content.Should().Be("Updated content");
        }

        [Test]
        public async Task UpdateComment_ByNonAuthor_ThrowsException()
        {
            var comment = new Models.Comment
                { CommentId = 1, AuthorId = "user-1", IsDeleted = false };
            var request = new UpdateCommentRequest("Hacked!");

            _repo.FindByCommentId(1).Returns(comment);

            var act = async () => await _sut.UpdateComment(1, request, "user-999");

            await act.Should().ThrowAsync<Exception>();
        }

        // ── DeleteComment ─────────────────────────────────────────────────────

        [Test]
        public async Task DeleteComment_ByAuthor_CallsRepoDelete()
        {
            var comment = new Models.Comment
                { CommentId = 1, AuthorId = "user-1", IsDeleted = false };

            _repo.FindByCommentId(1).Returns(comment);
            _repo.DeleteByCommentId(1).Returns(Task.CompletedTask);

            await _sut.DeleteComment(1, "user-1");

            await _repo.Received(1).DeleteByCommentId(1);
        }

        // ── GetCommentCount ───────────────────────────────────────────────────

        [Test]
        public async Task GetCommentCount_ReturnsCorrectCount()
        {
            _repo.CountByCardId(1).Returns(5);

            var result = await _sut.GetCommentCount(1);

            result.Should().Be(5);
        }

        // ── AddAttachment ─────────────────────────────────────────────────────

        [Test]
        public async Task AddAttachment_UploadsToS3AndSavesRecord()
        {
            var mockFile = Substitute.For<IFormFile>();
            mockFile.FileName.Returns("report.pdf");
            mockFile.ContentType.Returns("application/pdf");
            mockFile.Length.Returns(1024L);
            mockFile.OpenReadStream().Returns(new MemoryStream());

            var request    = new AddAttachmentRequest(CardId: 1, UploaderId: "user-1", File: mockFile);
            var attachment = new Attachment
            {
                AttachmentId = 1,
                CardId       = 1,
                FileName     = "report.pdf",
                FileUrl      = "https://s3.amazonaws.com/bucket/report.pdf"
            };

            _s3.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
               .Returns("https://s3.amazonaws.com/bucket/report.pdf");
            _repo.SaveAttachment(Arg.Any<Attachment>()).Returns(attachment);

            var result = await _sut.AddAttachment(request);

            result.Should().NotBeNull();
            result.FileName.Should().Be("report.pdf");
            result.FileUrl.Should().StartWith("https://");
        }
    }
}