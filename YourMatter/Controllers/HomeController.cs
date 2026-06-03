using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YourMatter.Models;
using YourMatter.Services.Contracts;

namespace YourMatter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;

        public HomeController(ILogger<HomeController> logger, IPostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var feedPosts = await _postService.GetFeedPostsAsync();
            return View(feedPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                if (statusCode == 404) return View("Error404");
                if (statusCode == 401) return View("Error401");
                if (statusCode == 400) return View("Error400");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
