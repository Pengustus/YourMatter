using NUnit.Framework;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class FriendServiceTests
    {
        private FriendService _service;
        private YourMatter.Data.Data.YourMatterDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create(Guid.NewGuid().ToString());
            _service = new FriendService(_context);

            _context.Users.AddRange(
                new ApplicationUser { Id = "user1", DisplayName = "User One", UserName = "user1", Email = "u1@test.com" },
                new ApplicationUser { Id = "user2", DisplayName = "User Two", UserName = "user2", Email = "u2@test.com" },
                new ApplicationUser { Id = "user3", DisplayName = "User Three", UserName = "user3", Email = "u3@test.com" }
            );
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        // --- SendRequestAsync ---

        [Test]
        public async Task SendRequestAsync_ValidRequest_ShouldReturnTrue()
        {
            var result = await _service.SendRequestAsync("user1", "user2");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SendRequestAsync_ShouldCreatePendingRequest()
        {
            await _service.SendRequestAsync("user1", "user2");

            var status = await _service.GetFriendshipStatusAsync("user1", "user2");
            Assert.That(status, Is.EqualTo(FriendRequestStatus.Pending));
        }

        [Test]
        public async Task SendRequestAsync_ToSelf_ShouldReturnFalse()
        {
            var result = await _service.SendRequestAsync("user1", "user1");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SendRequestAsync_DuplicateRequest_ShouldReturnFalse()
        {
            await _service.SendRequestAsync("user1", "user2");

            var result = await _service.SendRequestAsync("user1", "user2");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SendRequestAsync_ReverseAlreadyExists_ShouldReturnFalse()
        {
            await _service.SendRequestAsync("user2", "user1");

            var result = await _service.SendRequestAsync("user1", "user2");

            Assert.That(result, Is.False);
        }

        // --- AcceptRequestAsync ---

        [Test]
        public async Task AcceptRequestAsync_ValidRequest_ShouldReturnTrue()
        {
            await _service.SendRequestAsync("user1", "user2");

            var result = await _service.AcceptRequestAsync("user2", "user1");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task AcceptRequestAsync_ShouldSetStatusToAccepted()
        {
            await _service.SendRequestAsync("user1", "user2");
            await _service.AcceptRequestAsync("user2", "user1");

            var status = await _service.GetFriendshipStatusAsync("user1", "user2");
            Assert.That(status, Is.EqualTo(FriendRequestStatus.Accepted));
        }

        [Test]
        public async Task AcceptRequestAsync_NonExistentRequest_ShouldReturnFalse()
        {
            var result = await _service.AcceptRequestAsync("user2", "user1");

            Assert.That(result, Is.False);
        }

        // --- DeclineOrCancelRequestAsync ---

        [Test]
        public async Task DeclineOrCancelRequestAsync_ExistingRequest_ShouldReturnTrue()
        {
            await _service.SendRequestAsync("user1", "user2");

            var result = await _service.DeclineOrCancelRequestAsync("user1", "user2");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeclineOrCancelRequestAsync_ShouldRemoveRequest()
        {
            await _service.SendRequestAsync("user1", "user2");
            await _service.DeclineOrCancelRequestAsync("user1", "user2");

            var status = await _service.GetFriendshipStatusAsync("user1", "user2");
            Assert.That(status, Is.Null);
        }

        [Test]
        public async Task DeclineOrCancelRequestAsync_NonExistentRequest_ShouldReturnFalse()
        {
            var result = await _service.DeclineOrCancelRequestAsync("user1", "user2");

            Assert.That(result, Is.False);
        }

        // --- GetFriendsAsync ---

        [Test]
        public async Task GetFriendsAsync_ShouldReturnAcceptedFriends()
        {
            await _service.SendRequestAsync("user1", "user2");
            await _service.AcceptRequestAsync("user2", "user1");

            var friends = await _service.GetFriendsAsync("user1");

            Assert.That(friends.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetFriendsAsync_PendingRequest_ShouldNotAppearInFriends()
        {
            await _service.SendRequestAsync("user1", "user2");

            var friends = await _service.GetFriendsAsync("user1");

            Assert.That(friends.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetFriendsAsync_ShouldIncludeBothSentAndReceivedFriends()
        {
            // user1 sent to user2, user3 sent to user1 — both accepted
            await _service.SendRequestAsync("user1", "user2");
            await _service.AcceptRequestAsync("user2", "user1");
            await _service.SendRequestAsync("user3", "user1");
            await _service.AcceptRequestAsync("user1", "user3");

            var friends = await _service.GetFriendsAsync("user1");

            Assert.That(friends.Count(), Is.EqualTo(2));
        }

        // --- GetPendingRequestsAsync ---

        [Test]
        public async Task GetPendingRequestsAsync_ShouldReturnOnlyPendingRequests()
        {
            await _service.SendRequestAsync("user1", "user2");
            await _service.SendRequestAsync("user3", "user2");
            await _service.AcceptRequestAsync("user2", "user3"); // accept one

            var pending = await _service.GetPendingRequestsAsync("user2");

            Assert.That(pending.Count(), Is.EqualTo(1));
            Assert.That(pending.First().SenderId, Is.EqualTo("user1"));
        }

        // --- GetFriendshipStatusAsync ---

        [Test]
        public async Task GetFriendshipStatusAsync_NoRelationship_ShouldReturnNull()
        {
            var status = await _service.GetFriendshipStatusAsync("user1", "user2");

            Assert.That(status, Is.Null);
        }

        [Test]
        public async Task GetFriendshipStatusAsync_ShouldWorkBothDirections()
        {
            await _service.SendRequestAsync("user1", "user2");

            var statusForward = await _service.GetFriendshipStatusAsync("user1", "user2");
            var statusReverse = await _service.GetFriendshipStatusAsync("user2", "user1");

            Assert.That(statusForward, Is.EqualTo(FriendRequestStatus.Pending));
            Assert.That(statusReverse, Is.EqualTo(FriendRequestStatus.Pending));
        }
    }
}