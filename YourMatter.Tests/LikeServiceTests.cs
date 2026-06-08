using NUnit.Framework;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class LikeServiceTests
    {
        private LikeService _service;
        private YourMatter.Data.Data.YourMatterDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create(Guid.NewGuid().ToString());
            _service = new LikeService(_context);

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

        // --- ToggleLikeAsync ---

        [Test]
        public async Task ToggleLikeAsync_NewLike_ShouldReturnTrue()
        {
            var result = await _service.ToggleLikeAsync(1, "user1");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ToggleLikeAsync_NewLike_ShouldAddLikeToDatabase()
        {
            await _service.ToggleLikeAsync(1, "user1");

            var count = await _service.GetLikesCountAsync(1);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task ToggleLikeAsync_ExistingLike_ShouldReturnFalse()
        {
            await _service.ToggleLikeAsync(1, "user1");

            var result = await _service.ToggleLikeAsync(1, "user1");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ToggleLikeAsync_ExistingLike_ShouldRemoveLike()
        {
            await _service.ToggleLikeAsync(1, "user1");
            await _service.ToggleLikeAsync(1, "user1");

            var count = await _service.GetLikesCountAsync(1);
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task ToggleLikeAsync_NonExistentPost_ShouldReturnFalse()
        {
            var result = await _service.ToggleLikeAsync(9999, "user1");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ToggleLikeAsync_DeletedPost_ShouldReturnFalse()
        {
            _context.Posts.Add(new Post
            {
                Id = 2,
                AuthorId = "user1",
                Content = "deleted",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = true
            });
            await _context.SaveChangesAsync();

            var result = await _service.ToggleLikeAsync(2, "user1");

            Assert.That(result, Is.False);
        }

        // --- GetLikesCountAsync ---

        [Test]
        public async Task GetLikesCountAsync_NoLikes_ShouldReturnZero()
        {
            var count = await _service.GetLikesCountAsync(1);

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetLikesCountAsync_MultipleLikes_ShouldReturnCorrectCount()
        {
            _context.Users.Add(new ApplicationUser
            {
                Id = "user2",
                DisplayName = "User 2",
                UserName = "user2",
                Email = "user2@test.com"
            });
            await _context.SaveChangesAsync();

            await _service.ToggleLikeAsync(1, "user1");
            await _service.ToggleLikeAsync(1, "user2");

            var count = await _service.GetLikesCountAsync(1);
            Assert.That(count, Is.EqualTo(2));
        }

        // --- HasLikedAsync ---

        [Test]
        public async Task HasLikedAsync_WhenLiked_ShouldReturnTrue()
        {
            await _service.ToggleLikeAsync(1, "user1");

            var result = await _service.HasLikedAsync(1, "user1");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasLikedAsync_WhenNotLiked_ShouldReturnFalse()
        {
            var result = await _service.HasLikedAsync(1, "user1");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task HasLikedAsync_AfterUnlike_ShouldReturnFalse()
        {
            await _service.ToggleLikeAsync(1, "user1");
            await _service.ToggleLikeAsync(1, "user1");

            var result = await _service.HasLikedAsync(1, "user1");

            Assert.That(result, Is.False);
        }
    }
}