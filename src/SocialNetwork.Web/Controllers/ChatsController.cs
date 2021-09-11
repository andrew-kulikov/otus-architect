using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.Utils;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var chats = await _chatService.GetUserChatsAsync(userId);

            var model = new ChatsViewModel
            {
                Chats = chats
            };

            return View(model);
        }
    }
}