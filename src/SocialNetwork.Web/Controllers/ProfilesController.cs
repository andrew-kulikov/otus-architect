using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Core.Utils;
using SocialNetwork.Web.Utils;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class ProfilesController : UserActionControllerBase
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IFriendshipService _friendshipService;
        private readonly IMapper _mapper;

        public ProfilesController(
            IMapper mapper,
            IUserContext userContext, 
            IUserProfileRepository userProfileRepository,
            IFriendshipService friendshipService) : base(userContext)
        {
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
            _friendshipService = friendshipService;
        }

        public async Task<IActionResult> Index()
        {
            var allProfiles = await _userProfileRepository.GetAllUserProfilesAsync();
            var otherProfiles = allProfiles.Where(p => p.UserId != UserContext.CurrentUser.Id).ToList();

            return View(otherProfiles);
        }

        public IActionResult My()
        {
            return View(UserContext.CurrentUser.Profile);
        }

        public async Task<IActionResult> Profile(long userId)
        {
            var userProfile = await _userProfileRepository.GetUserProfileAsync(userId);
            var friendship = await _friendshipService.GetFriendshipAsync(User.GetUserId(), userId);

            return View(BuildProfileViewModel(userProfile, friendship));
        }

        private UserProfileViewModel BuildProfileViewModel(UserProfile userProfile, Friendship friendship)
        {
            var model = _mapper.Map<UserProfile, UserProfileViewModel>(userProfile);

            model.IsFriendshipInitiated = friendship != null;

            if (friendship != null)
            {
                model.FriendshipStatus = friendship.Status;
                model.IsUserRequester = userProfile.UserId == friendship.AddresseeId;
                model.FriendshipCreated = friendship.Created;
            }

            return model;
        }
    }
}