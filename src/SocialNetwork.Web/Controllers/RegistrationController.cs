using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Exceptions;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    [Route("[controller]")]
    public class RegistrationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationController(
            IAuthenticationService authenticationService, 
            IUserRepository userRepository, 
            IUserProfileRepository userProfileRepository, 
            IUnitOfWork unitOfWork)
        {
            _authenticationService = authenticationService;
            _userRepository = userRepository;
            _userProfileRepository = userProfileRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            var createdUser = new User
            {
                Email = model.Email,
                Username = model.Username
            };

            try
            {
                await _authenticationService.RegisterAsync(createdUser, model.Password);
                await _authenticationService.LoginAsync(model.Username, model.Password);

                return RedirectToAction("CreateProfile");
            }
            catch (UserNotFoundException e)
            {
                ModelState.AddModelError("", e.Message);
            }

            return View("Index", model);
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CreateProfile()
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile != null) return RedirectToAction("Index", "Home");

            return View(new CreateProfileViewModel { UserName = user.Username });
        }

        [Authorize]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateProfile(CreateProfileViewModel model)
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile != null) return RedirectToAction("Index", "Home");

            var profile = new UserProfile
            {
                Age = model.Age,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Interests = model.Interests,
                City = model.City
            };

            // TODO: Refactor - move to service
            await _userProfileRepository.AddUserProfileAsync(user, profile);
            await _unitOfWork.CommitAsync(); 

            return RedirectToAction("Index", "Home");
        }
    }
}