using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUserRepository _userRepository;

        public ProfilesController(IUserRepository userRepository, IUserProfileRepository userProfileRepository)
        {
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile == null) return RedirectToAction("CreateProfile", "Registration");

            var allProfiles = await _userProfileRepository.GetAllUserProfilesAsync();
            var otherProfiles = allProfiles.Where(p => p.UserId != user.Id).ToList();

            return View(otherProfiles);
        }

        public async Task<IActionResult> My()
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile == null) return RedirectToAction("CreateProfile", "Registration");

            return View(user.Profile);
        }
    }
}