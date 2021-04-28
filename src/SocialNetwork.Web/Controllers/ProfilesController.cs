using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Utils;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class ProfilesController : UserActionControllerBase
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IMapper _mapper;

        public ProfilesController(IUserContext userContext, IUserProfileRepository userProfileRepository, IMapper mapper) 
            : base(userContext)
        {
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var allProfiles = await _userProfileRepository.GetAllUserProfilesAsync();
            var otherProfiles = allProfiles.Where(p => p.UserId != UserContext.CurrentUser.Id).ToList();

            return View(otherProfiles);
        }

        public async Task<IActionResult> My()
        {
            return View(UserContext.CurrentUser.Profile);
        }

        public async Task<IActionResult> Profile(long userId)
        {
            var userProfile = await _userProfileRepository.GetUserProfileAsync(userId);
            var model = _mapper.Map<UserProfile, UserProfileViewModel>(userProfile);

            return View(model);
        }
    }
}