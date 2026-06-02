using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendService _friendService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendsController(IFriendService friendService, UserManager<ApplicationUser> userManager)
        {
            _friendService = friendService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var friends = await _friendService.GetFriendsAsync(user.Id);
            var pendingRequests = await _friendService.GetPendingRequestsAsync(user.Id);

            ViewBag.PendingRequests = pendingRequests;
            return View(friends);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(string receiverId, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool success = await _friendService.SendRequestAsync(user.Id, receiverId);
            if (success)
            {
                TempData["SuccessMessage"] = "Friend request sent!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not send friend request. You might already be friends or have a pending request.";
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(string senderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool success = await _friendService.AcceptRequestAsync(user.Id, senderId);
            if (success)
            {
                TempData["SuccessMessage"] = "Friend request accepted!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not accept friend request.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineOrCancelRequest(string otherUserId, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool success = await _friendService.DeclineOrCancelRequestAsync(user.Id, otherUserId);
            if (success)
            {
                TempData["SuccessMessage"] = "Friend request removed.";
            }

            return RedirectToLocal(returnUrl ?? Url.Action("Index"));
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
