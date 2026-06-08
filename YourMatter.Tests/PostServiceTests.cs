using NUnit.Framework;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class PostServiceTests
    {
        private PostService _service;
        private YourMatter.Data.Data.YourMatterDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create(Guid.NewGuid().ToString());
            _service = new PostService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // --- CreateAsync ---

        [Test]
        public async Task CreateAsync_ShouldAddPostToDatabase()
        {
            var post = await _service.CreateAsync("user1", "Hello world", null);

            Assert.That(post.Id, Is.GreaterThan(0));
            Assert.That(post.Content, Is.EqualTo("Hello world"));
            Assert.That(post.IsDeleted, Is.False);
        }

        [Test]
        public async Task CreateAsync_WithImageUrl_ShouldSaveImageUrl()
        {
            var post = await _service.CreateAsync("user1", "pic post", "http://img.com/x.jpg");

            Assert.That(post.ImageUrl, Is.EqualTo("http://img.com/x.jpg"));
        }

        // --- GetByIdAsync ---

        [Test]
        public async Task GetByIdAsync_ExistingPost_ShouldReturnPost()
        {
            var created = await _service.CreateAsync("user1", "test", null);

            var result = await _service.GetByIdAsync(created.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task GetByIdAsync_DeletedPost_ShouldReturnNull()
        {
            var created = await _service.CreateAsync("user1", "test", null);
            await _service.DeleteAsync(created.Id, "user1", false);

            var result = await _service.GetByIdAsync(created.Id);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
        {
            var result = await _service.GetByIdAsync(9999);

            Assert.That(result, Is.Null);
        }

        // --- DeleteAsync ---

        [Test]
        public async Task DeleteAsync_ByAuthor_ShouldSoftDelete()
        {
            var post = await _service.CreateAsync("user1", "test", null);

            var result = await _service.DeleteAsync(post.Id, "user1", false);

            Assert.That(result, Is.True);
            var deleted = await _context.Posts.FindAsync(post.Id);
            Assert.That(deleted!.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteAsync_ByAdmin_ShouldSoftDelete()
        {
            var post = await _service.CreateAsync("user1", "test", null);

            var result = await _service.DeleteAsync(post.Id, "adminUser", isAdmin: true);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteAsync_ByNonAuthorNonAdmin_ShouldReturnFalse()
        {
            var post = await _service.CreateAsync("user1", "test", null);

            var result = await _service.DeleteAsync(post.Id, "user2", false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteAsync_AlreadyDeleted_ShouldReturnFalse()
        {
            var post = await _service.CreateAsync("user1", "test", null);
            await _service.DeleteAsync(post.Id, "user1", false);

            var result = await _service.DeleteAsync(post.Id, "user1", false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteAsync_NonExistentPost_ShouldReturnFalse()
        {
            var result = await _service.DeleteAsync(9999, "user1", false);

            Assert.That(result, Is.False);
        }

        // --- GetFeedPostsAsync ---

        [Test]
        public async Task GetFeedPostsAsync_ShouldReturnOnlyNonDeletedPosts()
        {
            await _service.CreateAsync("user1", "visible", null);
            var deleted = await _service.CreateAsync("user1", "gone", null);
            await _service.DeleteAsync(deleted.Id, "user1", false);

            var feed = await _service.GetFeedPostsAsync();

            Assert.That(feed.Count(), Is.EqualTo(1));
            Assert.That(feed.First().Content, Is.EqualTo("visible"));
        }

        [Test]
        public async Task GetFeedPostsAsync_ShouldReturnPostsNewestFirst()
        {
            await _service.CreateAsync("user1", "first", null);
            await _service.CreateAsync("user1", "second", null);

            var feed = (await _service.GetFeedPostsAsync()).ToList();

            Assert.That(feed[0].Content, Is.EqualTo("second"));
            Assert.That(feed[1].Content, Is.EqualTo("first"));
        }

        // --- GetUserPostsAsync ---

        [Test]
        public async Task GetUserPostsAsync_ShouldReturnOnlyThatUsersPost()
        {
            await _service.CreateAsync("user1", "mine", null);
            await _service.CreateAsync("user2", "not mine", null);

            var posts = await _service.GetUserPostsAsync("user1");

            Assert.That(posts.Count(), Is.EqualTo(1));
            Assert.That(posts.First().AuthorId, Is.EqualTo("user1"));
        }
    }
}