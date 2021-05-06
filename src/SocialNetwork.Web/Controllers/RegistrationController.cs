using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
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

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CreateProfile()
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile != null) return RedirectToAction("Index", "Home");

            return View(new CreateProfileViewModel { UserName = user.Username });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            // TODO: Add validation
            var createdUser = new User
            {
                Email = model.Email,
                Username = model.Username
            };

            await _authenticationService.RegisterAsync(createdUser, model.Password);
            await _authenticationService.LoginAsync(model.Username, model.Password);

            return RedirectToAction("CreateProfile");
        }

        [Authorize]
        [HttpPost]
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