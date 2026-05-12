using FlowBoard.Comment.Infrastructure;
using FlowBoard.Comment.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Comment.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public CommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        // ── Comments ──────────────────────────────────────────────────────────

        // Query filter (IsDeleted = false) applied automatically — only top-level
        public async Task<List<Models.Comment>> FindByCardId(int cardId)
            => await _context.Comments
                .Where(c => c.CardId == cardId && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

        public async Task<List<Models.Comment>> FindByAuthorId(string authorId)
            => await _context.Comments
                .Where(c => c.AuthorId == authorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<Models.Comment?> FindByCommentId(int commentId)
            => await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

        public async Task<List<Models.Comment>> FindByParentCommentId(int parentCommentId)
            => await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

        public async Task<int> CountByCardId(int cardId)
            => await _context.Comments
                .CountAsync(c => c.CardId == cardId);

        // Soft-delete: bypass query filter to find + mark deleted
        public async Task DeleteByCommentId(int commentId)
        {
            var comment = await _context.Comments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment != null)
            {
                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Models.Comment> Save(Models.Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Models.Comment> Update(Models.Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        // ── Attachments ───────────────────────────────────────────────────────

        public async Task<List<Attachment>> FindAttachmentsByCardId(int cardId)
            => await _context.Attachments
                .Where(a => a.CardId == cardId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();

        public async Task<Attachment?> FindAttachmentById(int attachmentId)
            => await _context.Attachments
                .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);

        public async Task<Attachment> SaveAttachment(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}