using Microsoft.EntityFrameworkCore;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Services.Implementations
{
    public class FriendService : IFriendService
    {
        private readonly YourMatterDbContext _context;

        public FriendService(YourMatterDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SendRequestAsync(string senderId, string receiverId)
        {
            if (senderId == receiverId) return false;

            // Check if request already exists
            var existing = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => (fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                                           (fr.SenderId == receiverId && fr.ReceiverId == senderId));

            if (existing != null) return false; // Already sent or are friends

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                SentOn = DateTime.UtcNow
            };

            _context.FriendRequests.Add(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptRequestAsync(string receiverId, string senderId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.Status == FriendRequestStatus.Pending);

            if (request == null) return false;

            request.Status = FriendRequestStatus.Accepted;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineOrCancelRequestAsync(string userId, string otherUserId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => (fr.SenderId == userId && fr.ReceiverId == otherUserId) ||
                                           (fr.SenderId == otherUserId && fr.ReceiverId == userId));

            if (request == null) return false;

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetFriendsAsync(string userId)
        {
            // Get all accepted friend requests where user is sender
            var sentFriends = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Accepted)
                .Select(fr => fr.Receiver)
                .ToListAsync();

            // Get all accepted friend requests where user is receiver
            var receivedFriends = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Accepted)
                .Select(fr => fr.Sender)
                .ToListAsync();

            return sentFriends.Concat(receivedFriends);
        }

        public async Task<IEnumerable<FriendRequest>> GetPendingRequestsAsync(string userId)
        {
            return await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<FriendRequestStatus?> GetFriendshipStatusAsync(string userId, string otherUserId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => (fr.SenderId == userId && fr.ReceiverId == otherUserId) ||
                                           (fr.SenderId == otherUserId && fr.ReceiverId == userId));

            if (request == null) return null;

            return request.Status;
        }
    }
}
