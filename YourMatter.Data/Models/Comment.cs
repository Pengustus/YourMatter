namespace YourMatter.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public string AuthorId { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;
    }
}