using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using YourMatter.Data.Data;
using YourMatter.Data.Models;
using YourMatter.Services.Implementations;

namespace YourMatter.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        private YourMatterDbContext _context;
        private UserService _userService;
        private PostService _postService;
        private CommentService _commentService;
        private LikeService _likeService;
        private FriendService _friendService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<YourMatterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test.
                .Options;

            _context = new YourMatterDbContext(options);
            _context.Database.EnsureCreated();

            // Initialize Services
            _userService = new UserService(_context);
            _postService = new PostService(_context);
            _commentService = new CommentService(_context);
            _likeService = new LikeService(_context);
            _friendService = new FriendService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region User Service Tests

        [Test]
        public async Task GetByIdAsync_ShouldReturnCorrectUser()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.GetByIdAsync("u1");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DisplayName, Is.EqualTo("Alice"));
        }

        [Test]
        public async Task GetByEmailAsync_ShouldReturnCorrectUser()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.GetByEmailAsync("alice@test.com");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("u1"));
        }

        [Test]
        public async Task UpdateProfileAsync_ShouldUpdateUserSuccessfully()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com", Bio = "Old Bio" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var success = await _userService.UpdateProfileAsync("u1", "Alice Updated", "New Bio", "London", "/images/new.png");
            var updatedUser = await _context.Users.FindAsync("u1");

            Assert.That(success, Is.True);
            Assert.That(updatedUser.DisplayName, Is.EqualTo("Alice Updated"));
            Assert.That(updatedUser.Bio, Is.EqualTo("New Bio"));
            Assert.That(updatedUser.Location, Is.EqualTo("London"));
            Assert.That(updatedUser.ProfilePictureUrl, Is.EqualTo("/images/new.png"));
        }

        [Test]
        public async Task UpdateProfileAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            var success = await _userService.UpdateProfileAsync("non-existent", "Name", "Bio", "Loc", "Pic");
            Assert.That(success, Is.False);
        }

        [Test]
        public async Task SearchUsersAsync_ShouldFilterAndSortCorrectly()
        {
            var user1 = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com", Location = "Sofia", CreatedOn = DateTime.UtcNow.AddDays(-2) };
            var user2 = new ApplicationUser { Id = "u2", DisplayName = "Bob", Email = "bob@test.com", Location = "Plovdiv", CreatedOn = DateTime.UtcNow.AddDays(-1) };
            var user3 = new ApplicationUser { Id = "u3", DisplayName = "Charlie", Email = "charlie@test.com", Location = "Varna", CreatedOn = DateTime.UtcNow };

            _context.Users.AddRange(user1, user2, user3);
            await _context.SaveChangesAsync();

            // Search by term "Sofia"
            var (searchResult, searchCount) = await _userService.SearchUsersAsync("sofia", null, 1, 10);
            Assert.That(searchCount, Is.EqualTo(1));
            Assert.That(searchResult.First().DisplayName, Is.EqualTo("Alice"));

            // Sort by oldest
            var (oldestResult, _) = await _userService.SearchUsersAsync(null, "oldest", 1, 10);
            Assert.That(oldestResult.First().DisplayName, Is.EqualTo("Alice"));

            // Sort by newest
            var (newestResult, _) = await _userService.SearchUsersAsync(null, "newest", 1, 10);
            Assert.That(newestResult.First().DisplayName, Is.EqualTo("Charlie"));

            // Default sorting (alphabetical)
            var (defaultResult, _) = await _userService.SearchUsersAsync(null, null, 1, 10);
            var sortedNames = defaultResult.Select(u => u.DisplayName).ToList();
            Assert.That(sortedNames, Is.EqualTo(new List<string> { "Alice", "Bob", "Charlie" }));
        }

        #endregion

        #region Post Service Tests

        [Test]
        public async Task CreateAsync_ShouldAddPostSuccessfully()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var post = await _postService.CreateAsync("u1", "Hello World!", "/images/post.png");

            Assert.That(post, Is.Not.Null);
            Assert.That(post.Content, Is.EqualTo("Hello World!"));
            Assert.That(_context.Posts.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnActivePost()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var post = new Post { Id = 1, Author = user, Content = "My Post", IsDeleted = false };
            _context.Users.Add(user);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var result = await _postService.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("My Post"));
        }

        [Test]
        public async Task DeleteAsync_ByAuthor_ShouldMarkPostAsDeleted()
        {
            var post = new Post { Id = 1, AuthorId = "u1", Content = "Content", IsDeleted = false };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var success = await _postService.DeleteAsync(1, "u1", false);
            var deletedPost = await _context.Posts.FindAsync(1);

            Assert.That(success, Is.True);
            Assert.That(deletedPost.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteAsync_ByAdmin_ShouldMarkPostAsDeleted()
        {
            var post = new Post { Id = 1, AuthorId = "u1", Content = "Content", IsDeleted = false };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var success = await _postService.DeleteAsync(1, "adminId", true);
            var deletedPost = await _context.Posts.FindAsync(1);

            Assert.That(success, Is.True);
            Assert.That(deletedPost.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteAsync_ByNonAuthorNonAdmin_ShouldReturnFalse()
        {
            var post = new Post { Id = 1, AuthorId = "u1", Content = "Content", IsDeleted = false };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var success = await _postService.DeleteAsync(1, "otherId", false);

            Assert.That(success, Is.False);
        }

        [Test]
        public async Task GetFeedPostsAsync_ShouldReturnActivePostsOrderedByDate()
        {
            var author = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var post1 = new Post { Id = 1, Author = author, Content = "First", CreatedOn = DateTime.UtcNow.AddHours(-1), IsDeleted = false };
            var post2 = new Post { Id = 2, Author = author, Content = "Second", CreatedOn = DateTime.UtcNow, IsDeleted = false };
            var post3 = new Post { Id = 3, Author = author, Content = "Deleted", CreatedOn = DateTime.UtcNow.AddHours(1), IsDeleted = true };

            _context.Users.Add(author);
            _context.Posts.AddRange(post1, post2, post3);
            await _context.SaveChangesAsync();

            var feed = await _postService.GetFeedPostsAsync();

            Assert.That(feed.Count(), Is.EqualTo(2));
            Assert.That(feed.First().Id, Is.EqualTo(2)); // Order descending
        }

        [Test]
        public async Task GetUserPostsAsync_ShouldReturnAuthorPostsOnly()
        {
            var user1 = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var user2 = new ApplicationUser { Id = "u2", DisplayName = "Bob", Email = "bob@test.com" };
            var post1 = new Post { Id = 1, Author = user1, Content = "Alice Post", IsDeleted = false };
            var post2 = new Post { Id = 2, Author = user2, Content = "Bob Post", IsDeleted = false };

            _context.Users.AddRange(user1, user2);
            _context.Posts.AddRange(post1, post2);
            await _context.SaveChangesAsync();

            var userPosts = await _postService.GetUserPostsAsync("u1");

            Assert.That(userPosts.Count(), Is.EqualTo(1));
            Assert.That(userPosts.First().Content, Is.EqualTo("Alice Post"));
        }

        #endregion

        #region Comment Service Tests

        [Test]
        public async Task AddCommentAsync_ShouldAddCommentSuccessfully()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var post = new Post { Id = 1, Author = user, Content = "Post content", IsDeleted = false };
            
            _context.Users.Add(user);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var comment = await _commentService.AddCommentAsync(1, "u1", "My Comment");

            Assert.That(comment, Is.Not.Null);
            Assert.That(comment.Content, Is.EqualTo("My Comment"));
            Assert.That(_context.Comments.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnActiveComment()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var post = new Post { Id = 1, Author = user, Content = "Post", IsDeleted = false };
            var comment = new Comment { Id = 1, Post = post, Author = user, Content = "Comm", IsDeleted = false };

            _context.Users.Add(user);
            _context.Posts.Add(post);
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var result = await _commentService.GetByIdAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("Comm"));
        }

        [Test]
        public async Task DeleteCommentAsync_ByAuthor_ShouldMarkAsDeleted()
        {
            var comment = new Comment { Id = 1, AuthorId = "u1", Content = "Text", IsDeleted = false };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var success = await _commentService.DeleteCommentAsync(1, "u1", false);
            var deletedComment = await _context.Comments.FindAsync(1);

            Assert.That(success, Is.True);
            Assert.That(deletedComment.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteCommentAsync_ByAdmin_ShouldMarkAsDeleted()
        {
            var comment = new Comment { Id = 1, AuthorId = "u1", Content = "Text", IsDeleted = false };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var success = await _commentService.DeleteCommentAsync(1, "adminId", true);
            var deletedComment = await _context.Comments.FindAsync(1);

            Assert.That(success, Is.True);
            Assert.That(deletedComment.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteCommentAsync_ByNonAuthorNonAdmin_ShouldReturnFalse()
        {
            var comment = new Comment { Id = 1, AuthorId = "u1", Content = "Text", IsDeleted = false };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var success = await _commentService.DeleteCommentAsync(1, "otherId", false);

            Assert.That(success, Is.False);
        }

        #endregion

        #region Like Service Tests

        [Test]
        public async Task ToggleLikeAsync_ShouldAddAndRemoveLike()
        {
            var user = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var post = new Post { Id = 1, Author = user, Content = "Post", IsDeleted = false };

            _context.Users.Add(user);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Toggle on (Like)
            var liked = await _likeService.ToggleLikeAsync(1, "u1");
            Assert.That(liked, Is.True);
            Assert.That(await _likeService.GetLikesCountAsync(1), Is.EqualTo(1));
            Assert.That(await _likeService.HasLikedAsync(1, "u1"), Is.True);

            // Toggle off (Unlike)
            var unliked = await _likeService.ToggleLikeAsync(1, "u1");
            Assert.That(unliked, Is.False);
            Assert.That(await _likeService.GetLikesCountAsync(1), Is.EqualTo(0));
            Assert.That(await _likeService.HasLikedAsync(1, "u1"), Is.False);
        }

        [Test]
        public async Task ToggleLikeAsync_ForNonExistentPost_ShouldReturnFalse()
        {
            var result = await _likeService.ToggleLikeAsync(999, "u1");
            Assert.That(result, Is.False);
        }

        #endregion

        #region Friend Service Tests

        [Test]
        public async Task SendRequestAsync_ShouldCreatePendingRequest()
        {
            var user1 = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var user2 = new ApplicationUser { Id = "u2", DisplayName = "Bob", Email = "bob@test.com" };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var success = await _friendService.SendRequestAsync("u1", "u2");

            Assert.That(success, Is.True);
            var req = await _context.FriendRequests.FirstOrDefaultAsync();
            Assert.That(req, Is.Not.Null);
            Assert.That(req.SenderId, Is.EqualTo("u1"));
            Assert.That(req.ReceiverId, Is.EqualTo("u2"));
            Assert.That(req.Status, Is.EqualTo(FriendRequestStatus.Pending));
        }

        [Test]
        public async Task SendRequestAsync_ToSelf_ShouldReturnFalse()
        {
            var success = await _friendService.SendRequestAsync("u1", "u1");
            Assert.That(success, Is.False);
        }

        [Test]
        public async Task AcceptRequestAsync_ShouldChangeStatusToAccepted()
        {
            var user1 = new ApplicationUser { Id = "u1", DisplayName = "Alice", Email = "alice@test.com" };
            var user2 = new ApplicationUser { Id = "u2", DisplayName = "Bob", Email = "bob@test.com" };
            var req = new FriendRequest { SenderId = "u1", ReceiverId = "u2", Status = FriendRequestStatus.Pending };

            _context.Users.AddRange(user1, user2);
            _context.FriendRequests.Add(req);
            await _context.SaveChangesAsync();

            var success = await _friendService.AcceptRequestAsync("u2", "u1"); // Receiver accepts from sender
            var updatedReq = await _context.FriendRequests.FirstOrDefaultAsync();

            Assert.That(success, Is.True);
            Assert.That(updatedReq.Status, Is.EqualTo(FriendRequestStatus.Accepted));
        }

        [Test]
        public async Task AcceptRequestAsync_WithNonExistentRequest_ShouldReturnFalse()
        {
            var success = await _friendService.AcceptRequestAsync("u2", "u1");
            Assert.That(success, Is.False);
        }

        [Test]
        public async Task DeclineOrCancelRequestAsync_ShouldRemoveRequest()
        {
            var req = new FriendRequest { SenderId = "u1", ReceiverId = "u2", Status = FriendRequestStatus.Pending };
            _context.FriendRequests.Add(req);
            await _context.SaveChangesAsync();

            var success = await _friendService.DeclineOrCancelRequestAsync("u1", "u2");

            Assert.That(success, Is.True);
            Assert.That(_context.FriendRequests.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task GetFriendsAsync_ShouldReturnBothSentAndReceivedAcceptedFriends()
        {
            var alice = new ApplicationUser { Id = "alice", DisplayName = "Alice", Email = "a@test.com" };
            var bob = new ApplicationUser { Id = "bob", DisplayName = "Bob", Email = "b@test.com" };
            var charlie = new ApplicationUser { Id = "charlie", DisplayName = "Charlie", Email = "c@test.com" };

            // Alice is friends with Bob (Alice sent) and Charlie (Charlie sent)
            var req1 = new FriendRequest { SenderId = "alice", ReceiverId = "bob", Status = FriendRequestStatus.Accepted };
            var req2 = new FriendRequest { SenderId = "charlie", ReceiverId = "alice", Status = FriendRequestStatus.Accepted };

            _context.Users.AddRange(alice, bob, charlie);
            _context.FriendRequests.AddRange(req1, req2);
            await _context.SaveChangesAsync();

            var friends = await _friendService.GetFriendsAsync("alice");

            Assert.That(friends.Count(), Is.EqualTo(2));
            var friendNames = friends.Select(f => f.DisplayName).ToList();
            Assert.That(friendNames, Contains.Item("Bob"));
            Assert.That(friendNames, Contains.Item("Charlie"));
        }

        [Test]
        public async Task GetPendingRequestsAsync_ShouldReturnOnlyIncomingPendingRequests()
        {
            var alice = new ApplicationUser { Id = "alice", DisplayName = "Alice", Email = "a@test.com" };
            var bob = new ApplicationUser { Id = "bob", DisplayName = "Bob", Email = "b@test.com" };
            var charlie = new ApplicationUser { Id = "charlie", DisplayName = "Charlie", Email = "c@test.com" };

            var req1 = new FriendRequest { Sender = bob, SenderId = "bob", ReceiverId = "alice", Status = FriendRequestStatus.Pending };
            var req2 = new FriendRequest { Sender = alice, SenderId = "alice", ReceiverId = "charlie", Status = FriendRequestStatus.Pending }; // sent, not incoming

            _context.Users.AddRange(alice, bob, charlie);
            _context.FriendRequests.AddRange(req1, req2);
            await _context.SaveChangesAsync();

            var pending = await _friendService.GetPendingRequestsAsync("alice");

            Assert.That(pending.Count(), Is.EqualTo(1));
            Assert.That(pending.First().SenderId, Is.EqualTo("bob"));
        }

        [Test]
        public async Task GetFriendshipStatusAsync_ShouldReturnCorrectStatus()
        {
            var req = new FriendRequest { SenderId = "u1", ReceiverId = "u2", Status = FriendRequestStatus.Pending };
            _context.FriendRequests.Add(req);
            await _context.SaveChangesAsync();

            var status = await _friendService.GetFriendshipStatusAsync("u1", "u2");
            var statusReverse = await _friendService.GetFriendshipStatusAsync("u2", "u1");
            var statusNull = await _friendService.GetFriendshipStatusAsync("u1", "u3");

            Assert.That(status, Is.EqualTo(FriendRequestStatus.Pending));
            Assert.That(statusReverse, Is.EqualTo(FriendRequestStatus.Pending));
            Assert.That(statusNull, Is.Null);
        }

        #endregion
    }
}