using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly YourMatterDbContext _context;

        public UserService(YourMatterDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateProfileAsync(string id, string displayName, string? bio, string? location, string? profilePictureUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            user.DisplayName = displayName;
            user.Bio = bio;
            user.Location = location;
            if (!string.IsNullOrWhiteSpace(profilePictureUrl))
            {
                user.ProfilePictureUrl = profilePictureUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> SearchUsersAsync(string? searchTerm, string? sortBy, int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(u => u.DisplayName.ToLower().Contains(searchTerm) || 
                                         (u.Location != null && u.Location.ToLower().Contains(searchTerm)) ||
                                         u.Email!.ToLower().Contains(searchTerm));
            }

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "newest" => query.OrderByDescending(u => u.CreatedOn),
                "oldest" => query.OrderBy(u => u.CreatedOn),
                "name_desc" => query.OrderByDescending(u => u.DisplayName),
                _ => query.OrderBy(u => u.DisplayName) // default: alphabetical
            };

            int totalCount = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            // Delete user's comments on other posts
            var comments = _context.Comments.Where(c => c.AuthorId == id);
            _context.Comments.RemoveRange(comments);

            // Delete user's likes on other posts
            var likes = _context.Likes.Where(l => l.UserId == id);
            _context.Likes.RemoveRange(likes);

            // Delete user's friend requests
            var friendRequests = _context.FriendRequests.Where(fr => fr.SenderId == id || fr.ReceiverId == id);
            _context.FriendRequests.RemoveRange(friendRequests);

            // Delete user's posts
            var posts = _context.Posts.Where(p => p.AuthorId == id);
            _context.Posts.RemoveRange(posts);

            // Delete the user record
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
