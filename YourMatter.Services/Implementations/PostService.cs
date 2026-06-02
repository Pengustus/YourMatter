using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Services.Implementations
{
    public class PostService : IPostService
    {
        private readonly YourMatterDbContext _context;

        public PostService(YourMatterDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Post> CreateAsync(string authorId, string content, string? imageUrl)
        {
            var post = new Post
            {
                AuthorId = authorId,
                Content = content,
                ImageUrl = imageUrl,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null || post.IsDeleted) return false;

            // Only author or admin can delete post
            if (post.AuthorId != userId && !isAdmin) return false;

            post.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Post>> GetFeedPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Author)
                .Include(p => p.Likes)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetUserPostsAsync(string userId)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Author)
                .Include(p => p.Likes)
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }
    }
}
