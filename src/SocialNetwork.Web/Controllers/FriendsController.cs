using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Services;
using SocialNetwork.Core.Utils;
using SocialNetwork.Web.Utils;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class FriendsController : UserActionControllerBase
    {
        private readonly IFriendshipService _friendshipService;

        public FriendsController(IUserContext userContext, IFriendshipService friendshipService) : base(userContext)
        {
            _friendshipService = friendshipService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var friendships = await _friendshipService.GetFriendsAsync(userId);

            var model = BuildFriendsViewModel(friendships, userId);

            return View(model);
        }

        private static FriendsViewModel BuildFriendsViewModel(ICollection<Friendship> friendships, long userId)
        {
            return new FriendsViewModel
            {
                Friends = friendships
                    .Where(f => f.Status == FriendshipStatus.RequestAccepted)
                    .Select(f => f.AddresseeId == userId ? f.Requester : f.Addressee)
                    .ToList(),

                IncomingRequests = friendships
                    .Where(f => f.Status == FriendshipStatus.RequestSent && f.AddresseeId == userId)
                    .ToList(),

                PendingRequests = friendships
                    .Where(f => f.Status == FriendshipStatus.RequestSent && f.RequesterId == userId)
                    .ToList()
            };
        }

        [HttpPost]
        [Route("api/friends")]
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

        [HttpPut]
        [Route("api/friends")]
        public async Task<IActionResult> AcceptFriend(long userId)
        {
            try
            {
                await _friendshipService.AcceptAsync(userId, User.GetUserId());

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("api/friends/{friendId}")]
        public async Task<IActionResult> DeleteFriend(long friendId)
        {
            try
            {
                await _friendshipService.RemoveAsync(User.GetUserId(), friendId);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}