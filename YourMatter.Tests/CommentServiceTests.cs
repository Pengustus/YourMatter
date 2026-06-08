using NUnit.Framework;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class CommentServiceTests
    {
        private CommentService _service;
        private YourMatter.Data.Data.YourMatterDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create(Guid.NewGuid().ToString());
            _service = new CommentService(_context);

            _context.Users.Add(new ApplicationUser
            {
                Id = "user1",
                DisplayName = "Test User",
                UserName = "testuser",
                Email = "test@test.com"
            });
            _context.Posts.Add(new Post
            {
                Id = 1,
                AuthorId = "user1",
                Content = "test post",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            });
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        // --- AddCommentAsync ---

        [Test]
        public async Task AddCommentAsync_ShouldAddCommentToDatabase()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "nice post!");

            Assert.That(comment.Id, Is.GreaterThan(0));
            Assert.That(comment.Content, Is.EqualTo("nice post!"));
            Assert.That(comment.IsDeleted, Is.False);
            Assert.That(comment.PostId, Is.EqualTo(1));
        }

        [Test]
        public async Task AddCommentAsync_ShouldSetCorrectAuthorAndPost()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "hello");

            Assert.That(comment.AuthorId, Is.EqualTo("user1"));
            Assert.That(comment.PostId, Is.EqualTo(1));
        }

        // --- GetByIdAsync ---

        [Test]
        public async Task GetByIdAsync_ExistingComment_ShouldReturnComment()
        {
            var created = await _service.AddCommentAsync(1, "user1", "test");

            var result = await _service.GetByIdAsync(created.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task GetByIdAsync_DeletedComment_ShouldReturnNull()
        {
            var created = await _service.AddCommentAsync(1, "user1", "test");
            await _service.DeleteCommentAsync(created.Id, "user1", false);

            var result = await _service.GetByIdAsync(created.Id);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
        {
            var result = await _service.GetByIdAsync(9999);

            Assert.That(result, Is.Null);
        }

        // --- DeleteCommentAsync ---

        [Test]
        public async Task DeleteCommentAsync_ByAuthor_ShouldSoftDelete()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "test");

            var result = await _service.DeleteCommentAsync(comment.Id, "user1", false);

            Assert.That(result, Is.True);
            var deleted = await _context.Comments.FindAsync(comment.Id);
            Assert.That(deleted!.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteCommentAsync_ByAdmin_ShouldSoftDelete()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "test");

            var result = await _service.DeleteCommentAsync(comment.Id, "adminUser", isAdmin: true);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteCommentAsync_ByNonAuthorNonAdmin_ShouldReturnFalse()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "test");

            var result = await _service.DeleteCommentAsync(comment.Id, "user2", false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteCommentAsync_AlreadyDeleted_ShouldReturnFalse()
        {
            var comment = await _service.AddCommentAsync(1, "user1", "test");
            await _service.DeleteCommentAsync(comment.Id, "user1", false);

            var result = await _service.DeleteCommentAsync(comment.Id, "user1", false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteCommentAsync_NonExistentComment_ShouldReturnFalse()
        {
            var result = await _service.DeleteCommentAsync(9999, "user1", false);

            Assert.That(result, Is.False);
        }
    }
}