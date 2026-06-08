using NUnit.Framework;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private UserService _service;
        private YourMatter.Data.Data.YourMatterDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create(Guid.NewGuid().ToString());
            _service = new UserService(_context);

            _context.Users.AddRange(
                new ApplicationUser { Id = "user1", DisplayName = "Alice Smith", UserName = "alice", Email = "alice@test.com", Location = "London", CreatedOn = DateTime.UtcNow.AddDays(-2) },
                new ApplicationUser { Id = "user2", DisplayName = "Bob Jones", UserName = "bob", Email = "bob@test.com", Location = "Paris", CreatedOn = DateTime.UtcNow.AddDays(-1) },
                new ApplicationUser { Id = "user3", DisplayName = "Charlie Brown", UserName = "charlie", Email = "charlie@test.com", Location = null, CreatedOn = DateTime.UtcNow }
            );
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        // --- GetByIdAsync ---

        [Test]
        public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
        {
            var result = await _service.GetByIdAsync("user1");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.DisplayName, Is.EqualTo("Alice Smith"));
        }

        [Test]
        public async Task GetByIdAsync_NonExistentUser_ShouldReturnNull()
        {
            var result = await _service.GetByIdAsync("nobody");

            Assert.That(result, Is.Null);
        }

        // --- GetByEmailAsync ---

        [Test]
        public async Task GetByEmailAsync_ExistingEmail_ShouldReturnUser()
        {
            var result = await _service.GetByEmailAsync("bob@test.com");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo("user2"));
        }

        [Test]
        public async Task GetByEmailAsync_NonExistentEmail_ShouldReturnNull()
        {
            var result = await _service.GetByEmailAsync("nobody@test.com");

            Assert.That(result, Is.Null);
        }

        // --- UpdateProfileAsync ---

        [Test]
        public async Task UpdateProfileAsync_ExistingUser_ShouldReturnTrue()
        {
            var result = await _service.UpdateProfileAsync("user1", "Alice Updated", "My bio", "NYC", null);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UpdateProfileAsync_ShouldUpdateFields()
        {
            await _service.UpdateProfileAsync("user1", "Alice Updated", "My bio", "NYC", "http://img.com/pic.jpg");

            var user = await _service.GetByIdAsync("user1");
            Assert.That(user!.DisplayName, Is.EqualTo("Alice Updated"));
            Assert.That(user.Bio, Is.EqualTo("My bio"));
            Assert.That(user.Location, Is.EqualTo("NYC"));
            Assert.That(user.ProfilePictureUrl, Is.EqualTo("http://img.com/pic.jpg"));
        }

        [Test]
        public async Task UpdateProfileAsync_NullImageUrl_ShouldNotOverwriteExistingPicture()
        {
            // Set an initial picture
            await _service.UpdateProfileAsync("user1", "Alice", null, null, "http://img.com/original.jpg");

            // Update without changing picture
            await _service.UpdateProfileAsync("user1", "Alice Updated", null, null, null);

            var user = await _service.GetByIdAsync("user1");
            Assert.That(user!.ProfilePictureUrl, Is.EqualTo("http://img.com/original.jpg"));
        }

        [Test]
        public async Task UpdateProfileAsync_NonExistentUser_ShouldReturnFalse()
        {
            var result = await _service.UpdateProfileAsync("nobody", "Name", null, null, null);

            Assert.That(result, Is.False);
        }

        // --- SearchUsersAsync ---

        [Test]
        public async Task SearchUsersAsync_NoSearchTerm_ShouldReturnAllUsers()
        {
            var (users, total) = await _service.SearchUsersAsync(null, null, 1, 10);

            Assert.That(total, Is.EqualTo(3));
        }

        [Test]
        public async Task SearchUsersAsync_ByDisplayName_ShouldFilterCorrectly()
        {
            var (users, total) = await _service.SearchUsersAsync("alice", null, 1, 10);

            Assert.That(total, Is.EqualTo(1));
            Assert.That(users.First().Id, Is.EqualTo("user1"));
        }

        [Test]
        public async Task SearchUsersAsync_ByEmail_ShouldFilterCorrectly()
        {
            var (users, total) = await _service.SearchUsersAsync("bob@test.com", null, 1, 10);

            Assert.That(total, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchUsersAsync_ByLocation_ShouldFilterCorrectly()
        {
            var (users, total) = await _service.SearchUsersAsync("london", null, 1, 10);

            Assert.That(total, Is.EqualTo(1));
            Assert.That(users.First().Id, Is.EqualTo("user1"));
        }

        [Test]
        public async Task SearchUsersAsync_NoMatch_ShouldReturnEmpty()
        {
            var (users, total) = await _service.SearchUsersAsync("zzznomatch", null, 1, 10);

            Assert.That(total, Is.EqualTo(0));
            Assert.That(users.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task SearchUsersAsync_SortByNewest_ShouldReturnNewestFirst()
        {
            var (users, _) = await _service.SearchUsersAsync(null, "newest", 1, 10);

            Assert.That(users.First().Id, Is.EqualTo("user3"));
        }

        [Test]
        public async Task SearchUsersAsync_SortByOldest_ShouldReturnOldestFirst()
        {
            var (users, _) = await _service.SearchUsersAsync(null, "oldest", 1, 10);

            Assert.That(users.First().Id, Is.EqualTo("user1"));
        }

        [Test]
        public async Task SearchUsersAsync_Paging_ShouldRespectPageSize()
        {
            var (users, total) = await _service.SearchUsersAsync(null, null, 1, 2);

            Assert.That(total, Is.EqualTo(3));
            Assert.That(users.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task SearchUsersAsync_SecondPage_ShouldReturnRemainingUsers()
        {
            var (users, _) = await _service.SearchUsersAsync(null, null, 2, 2);

            Assert.That(users.Count(), Is.EqualTo(1));
        }
    }
}