using YourMatter.Data.Models;

namespace YourMatter.Services.Contracts
{
    public interface IFriendService
    {
        Task<bool> SendRequestAsync(string senderId, string receiverId);
        Task<bool> AcceptRequestAsync(string receiverId, string senderId);
        Task<bool> DeclineOrCancelRequestAsync(string userId, string otherUserId);
        Task<IEnumerable<ApplicationUser>> GetFriendsAsync(string userId);
        Task<IEnumerable<FriendRequest>> GetPendingRequestsAsync(string userId);
        Task<FriendRequestStatus?> GetFriendshipStatusAsync(string userId, string otherUserId);
    }
}
