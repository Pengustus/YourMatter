using YourMatter.Data.Models;

namespace YourMatter.Services.Contracts
{
    public interface ICommentService
    {
        Task<Comment?> GetByIdAsync(int id);
        Task<Comment> AddCommentAsync(int postId, string authorId, string content);
        Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin);
    }
}
