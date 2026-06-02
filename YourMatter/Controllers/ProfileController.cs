using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMatter.Data.Models;
using YourMatter.Models.Profile;
using YourMatter.Services.Contracts;

namespace YourMatter.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IFriendService _friendService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(IUserService userService, IPostService postService, IFriendService friendService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _postService = postService;
            _friendService = friendService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? id)
        {
            ApplicationUser? user = null;

            if (string.IsNullOrEmpty(id))
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    user = await _userManager.GetUserAsync(User);
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                user = await _userService.GetByIdAsync(id);
            }

            if (user == null)
            {
                return NotFound();
            }

            var posts = await _postService.GetUserPostsAsync(user.Id);
            var friends = await _friendService.GetFriendsAsync(user.Id);

            FriendRequestStatus? friendshipStatus = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id != user.Id)
                {
                    friendshipStatus = await _friendService.GetFriendshipStatusAsync(currentUser.Id, user.Id);
                }
            }

            ViewBag.Posts = posts;
            ViewBag.Friends = friends.Take(6).ToList(); // Top 6 friends
            ViewBag.FriendshipStatus = friendshipStatus;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Location = user.Location,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var success = await _userService.UpdateProfileAsync(user.Id, model.DisplayName, model.Bio, model.Location, model.ProfilePictureUrl);

                if (success)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the profile.");
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? searchTerm, string? sortBy, int page = 1)
        {
            int pageSize = 10;
            var (users, totalCount) = await _userService.SearchUsersAsync(searchTerm, sortBy, page, pageSize);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(users);
        }
    }
}
