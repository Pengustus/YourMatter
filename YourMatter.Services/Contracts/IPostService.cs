using YourMatter.Data.Models;

namespace YourMatter.Services.Contracts
{
    public interface IPostService
    {
        Task<Post?> GetByIdAsync(int id);
        Task<Post> CreateAsync(string authorId, string content, string? imageUrl);
        Task<bool> DeleteAsync(int id, string userId, bool isAdmin);
        Task<IEnumerable<Post>> GetFeedPostsAsync();
        Task<IEnumerable<Post>> GetUserPostsAsync(string userId);
    }
}
