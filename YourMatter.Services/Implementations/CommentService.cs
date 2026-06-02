using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly YourMatterDbContext _context;

        public CommentService(YourMatterDbContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Comment> AddCommentAsync(int postId, string authorId, string content)
        {
            var comment = new Comment
            {
                PostId = postId,
                AuthorId = authorId,
                Content = content,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null || comment.IsDeleted) return false;

            // Only author or admin can delete comment
            if (comment.AuthorId != userId && !isAdmin) return false;

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
