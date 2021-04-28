using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.Utils;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/friends")]
    public class FriendsController : Controller
    {
        private readonly IFriendshipService _friendshipService;

        public FriendsController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddFriend(long userId)
        {
            try
            {
                await _friendshipService.AddAsync(User.GetUserId(), userId);

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}