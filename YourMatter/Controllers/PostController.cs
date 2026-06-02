using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(IPostService postService, ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Post content cannot be empty.";
                return RedirectToLocal(returnUrl);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _postService.CreateAsync(user.Id, content, null);
            TempData["SuccessMessage"] = "Post created successfully!";
            
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            var success = await _postService.DeleteAsync(id, user.Id, isAdmin);

            if (success)
            {
                TempData["SuccessMessage"] = "Post deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete the post.";
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToLocal(returnUrl);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _commentService.AddCommentAsync(postId, user.Id, content);
            TempData["SuccessMessage"] = "Comment added!";

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");
            var success = await _commentService.DeleteCommentAsync(id, user.Id, isAdmin);

            if (success)
            {
                TempData["SuccessMessage"] = "Comment deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete the comment.";
            }

            return RedirectToLocal(returnUrl);
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
