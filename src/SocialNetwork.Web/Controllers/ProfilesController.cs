using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Core.Utils;
using SocialNetwork.Infrastructure.Services;
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
        private readonly IUserProfileSearchService _profileSearchService;

        public ProfilesController(
            IMapper mapper,
            IUserContext userContext, 
            IUserProfileRepository userProfileRepository,
            IFriendshipService friendshipService, 
            IUserProfileSearchService profileSearchService) : base(userContext)
        {
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
            _friendshipService = friendshipService;
            _profileSearchService = profileSearchService;
        }

        public async Task<IActionResult> Index(int page, int pageSize)
        {
            (page, pageSize) = ValidatePage(page, pageSize);

            var allProfiles = await _userProfileRepository.GetAllUserProfilesAsync(page, pageSize);
            var profiles = FilterOtherUsers(allProfiles);

            return View(new UserProfilesViewModel
            {
                Page = page,
                PageSize = pageSize,
                Profiles = profiles
            });
        }

        public async Task<IActionResult> Search(string query, int page, int pageSize)
        {
            (page, pageSize) = ValidatePage(page, pageSize);

            var allProfiles = await _profileSearchService.SearchUserProfilesAsync(query, page, pageSize);
            var profiles = FilterOtherUsers(allProfiles);

            return View(new UserProfilesSearchViewModel
            {
                Query = query,
                Page = page,
                PageSize = pageSize,
                Profiles = profiles
            });
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

        private ICollection<UserProfile> FilterOtherUsers(ICollection<UserProfile> profiles) =>
            profiles.Where(p => p.UserId != UserContext.CurrentUser.Id).ToList();

        private (int page, int pageSize) ValidatePage(int page, int pageSize)
        {
            const int defaultPageSize = 30;

            if (page <= 0) page = 0;
            if (pageSize <= 0 || pageSize > 100) pageSize = defaultPageSize;

            return (page, pageSize);
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