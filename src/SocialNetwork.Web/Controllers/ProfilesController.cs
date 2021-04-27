using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Utils;

namespace SocialNetwork.Web.Controllers
{
    public class UserActionControllerBase : Controller
    {
        protected readonly IUserContext UserContext;

        public UserActionControllerBase(IUserContext userContext)
        {
            UserContext = userContext;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (UserContext.CurrentUser.Profile == null)
            {
                context.Result = new RedirectToActionResult("CreateProfile", "Registration", context.RouteData);
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }

    [Authorize]
    public class ProfilesController : UserActionControllerBase
    {
        private readonly IUserProfileRepository _userProfileRepository;

        public ProfilesController(IUserContext userContext, IUserProfileRepository userProfileRepository) 
            : base(userContext)
        {
            _userProfileRepository = userProfileRepository;
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
    }
}