using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
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

        [HttpGet]
        public async Task<IActionResult> Chat(long chatId, int page = 0, int pageSize = 100)
        {
            var userId = User.GetUserId();
            var peer = await _chatService.FindPeerAsync(chatId, userId);
            var messages = await _chatService.GetMessagesAsync(chatId, page, pageSize);

            var model = new ChatViewModel
            {
                ChatId = chatId,
                Peer = peer.UserProfile,
                UserId = userId,

                Page = page,
                PageSize = pageSize,
                Messages = messages
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat(long userId)
        {
            await _chatService.CreateChatAsync(User.GetUserId(), userId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageViewModel model)
        {
            await _chatService.CreateMessageAsync(model.Text, model.ChatId, User.GetUserId());

            return Ok();
        }
    }
}