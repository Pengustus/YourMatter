using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Services.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly YourMatterDbContext _context;

        public LikeService(YourMatterDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleLikeAsync(int postId, string userId)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId && !p.IsDeleted);
            if (!postExists) return false;

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false; // Liked toggled off (unliked)
            }
            else
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow
                };
                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return true; // Liked toggled on (liked)
            }
        }

        public async Task<int> GetLikesCountAsync(int postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task<bool> HasLikedAsync(int postId, string userId)
        {
            return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }
    }
}
