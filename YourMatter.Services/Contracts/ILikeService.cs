namespace YourMatter.Services.Contracts
{
    public interface ILikeService
    {
        Task<bool> ToggleLikeAsync(int postId, string userId);
        Task<int> GetLikesCountAsync(int postId);
        Task<bool> HasLikedAsync(int postId, string userId);
    }
}
