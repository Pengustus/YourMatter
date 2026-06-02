using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMatter.Data.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikesApiController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LikesApiController(ILikeService likeService, UserManager<ApplicationUser> userManager)
        {
            _likeService = likeService;
            _userManager = userManager;
        }

        [HttpPost("toggle/{postId}")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "You must be logged in to like posts." });
            }

            bool liked = await _likeService.ToggleLikeAsync(postId, user.Id);
            int newCount = await _likeService.GetLikesCountAsync(postId);

            return Ok(new { liked = liked, count = newCount });
        }
    }
}
