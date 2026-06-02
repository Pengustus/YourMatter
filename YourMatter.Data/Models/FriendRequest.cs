namespace YourMatter.Data.Models
{
    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Declined
    }

    public class FriendRequest
    {
        public int Id { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime SentOn { get; set; } = DateTime.UtcNow;

        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = null!;
        public ApplicationUser Receiver { get; set; } = null!;
    }
}