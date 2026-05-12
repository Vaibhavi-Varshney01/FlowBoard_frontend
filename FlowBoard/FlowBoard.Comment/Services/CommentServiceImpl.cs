using FlowBoard.Comment.Events;
using FlowBoard.Comment.Infrastructure;
using FlowBoard.Comment.Storage;
using FlowBoard.Comment.Models;
using FlowBoard.Comment.Repositories;
using MassTransit;

namespace FlowBoard.Comment.Services
{
    public class CommentServiceImpl : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IS3Service         _s3Service;
        private readonly IPublishEndpoint   _publishEndpoint; // MassTransit RabbitMQ

        public CommentServiceImpl(
            ICommentRepository commentRepository,
            IS3Service         s3Service,
            IPublishEndpoint   publishEndpoint)
        {
            _commentRepository = commentRepository;
            _s3Service         = s3Service;
            _publishEndpoint   = publishEndpoint;
        }

        // ── Comment CRUD ──────────────────────────────────────────────────────

        public async Task<Models.Comment> AddComment(AddCommentRequest request)
        {
            // If reply, validate parent exists and belongs to same card
            if (request.ParentCommentId.HasValue)
            {
                var parent = await _commentRepository.FindByCommentId(request.ParentCommentId.Value)
                    ?? throw new KeyNotFoundException(
                        $"Parent comment {request.ParentCommentId} not found.");

                if (parent.CardId != request.CardId)
                    throw new InvalidOperationException(
                        "Reply must belong to the same card as the parent comment.");
            }

            var comment = new Models.Comment
            {
                CardId          = request.CardId,
                AuthorId        = request.AuthorId,
                Content         = request.Content,
                ParentCommentId = request.ParentCommentId,
                IsDeleted       = false,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };

            var saved = await _commentRepository.Save(comment);

            // RabbitMQ: Notify card watchers and @mentioned users
            await _publishEndpoint.Publish(new CommentAddedEvent
            {
                CommentId       = saved.CommentId,
                CardId          = saved.CardId,
                AuthorId        = saved.AuthorId,
                Content         = saved.Content,
                ParentCommentId = saved.ParentCommentId,
                OccurredAt      = DateTime.UtcNow
            });

            Console.WriteLine(
                $"[RABBITMQ] CommentAddedEvent published for comment {saved.CommentId} on card {saved.CardId}");

            return saved;
        }

        public async Task<Models.Comment> GetCommentById(int commentId)
            => await _commentRepository.FindByCommentId(commentId)
               ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

        public async Task<List<Models.Comment>> GetByCard(int cardId)
            => await _commentRepository.FindByCardId(cardId);

        public async Task<List<Models.Comment>> GetReplies(int parentCommentId)
        {
            _ = await _commentRepository.FindByCommentId(parentCommentId)
                ?? throw new KeyNotFoundException($"Comment {parentCommentId} not found.");

            return await _commentRepository.FindByParentCommentId(parentCommentId);
        }

        public async Task<Models.Comment> UpdateComment(
            int commentId,
            UpdateCommentRequest request,
            string requestingUserId)
        {
            var comment = await _commentRepository.FindByCommentId(commentId)
                ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

            if (comment.AuthorId != requestingUserId)
                throw new UnauthorizedAccessException("Only the author can edit this comment.");

            comment.Content   = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            return await _commentRepository.Update(comment);
        }

        public async Task DeleteComment(int commentId, string requestingUserId)
        {
            var comment = await _commentRepository.FindByCommentId(commentId)
                ?? throw new KeyNotFoundException($"Comment {commentId} not found.");

            if (comment.AuthorId != requestingUserId)
                throw new UnauthorizedAccessException("Only the author can delete this comment.");

            // Soft-delete: IsDeleted = true, history preserved for moderation
            await _commentRepository.DeleteByCommentId(commentId);
        }

        public async Task<int> GetCommentCount(int cardId)
            => await _commentRepository.CountByCardId(cardId);

        // ── Attachment — AWS S3 ───────────────────────────────────────────────

        public async Task<Attachment> AddAttachment(AddAttachmentRequest request)
        {
            var file = request.File;

            // Upload file to AWS S3 — returns public URL
            string fileUrl;
            await using (var stream = file.OpenReadStream())
            {
                fileUrl = await _s3Service.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType);
            }

            var attachment = new Attachment
            {
                CardId     = request.CardId,
                UploaderId = request.UploaderId,
                FileName   = file.FileName,
                FileUrl    = fileUrl,                             // S3 URL
                FileType   = file.ContentType,
                SizeKb     = file.Length / 1024,
                UploadedAt = DateTime.UtcNow
            };

            var saved = await _commentRepository.SaveAttachment(attachment);

            // RabbitMQ: Notify card watchers about new attachment
            await _publishEndpoint.Publish(new AttachmentAddedEvent
            {
                AttachmentId = saved.AttachmentId,
                CardId       = saved.CardId,
                UploaderId   = saved.UploaderId,
                FileName     = saved.FileName,
                FileUrl      = saved.FileUrl,
                OccurredAt   = DateTime.UtcNow
            });

            Console.WriteLine(
                $"[RABBITMQ] AttachmentAddedEvent published for attachment {saved.AttachmentId} on card {saved.CardId}");

            return saved;
        }

        public async Task<List<Attachment>> GetAttachmentsByCard(int cardId)
            => await _commentRepository.FindAttachmentsByCardId(cardId);

        public async Task DeleteAttachment(int attachmentId, string requestingUserId)
        {
            var attachment = await _commentRepository.FindAttachmentById(attachmentId)
                ?? throw new KeyNotFoundException($"Attachment {attachmentId} not found.");

            if (attachment.UploaderId != requestingUserId)
                throw new UnauthorizedAccessException("Only the uploader can delete this attachment.");

            // Delete from AWS S3 first
            await _s3Service.DeleteFileAsync(attachment.FileUrl);

            // Then delete from DB
            await _commentRepository.DeleteAttachment(attachmentId);
        }
    }
}