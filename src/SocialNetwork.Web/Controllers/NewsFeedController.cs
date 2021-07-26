using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Services;
using SocialNetwork.Core.Utils;
using SocialNetwork.Web.Utils;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Route("feed")]
    public class NewsFeedController : UserActionControllerBase
    {
        private readonly IUserPostService _userPostService;

        public NewsFeedController(IUserContext userContext, IUserPostService userPostService) : base(userContext)
        {
            _userPostService = userPostService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Feed()
        {
            var news = await _userPostService.GetNewsFeedAsync(HttpContext.User.GetUserId());

            return View("Index", news);
        }

        [HttpGet]
        [Route("new")]
        public IActionResult CreatePost() => View("CreatePost");

        [HttpPost]
        [Route("new")]
        public async Task<IActionResult> CreatePost(CreatePostViewModel model)
        {
            await _userPostService.AddPostAsync(model.Text, HttpContext.User.GetUserId());

            return RedirectToAction("Feed");
        }
    }
}