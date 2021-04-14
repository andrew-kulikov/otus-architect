using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IUserRepository _userRepository;

        public RegistrationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            // TODO: Add validation
            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                RegisteredAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            await _userRepository.AddUserAsync(user);

            return View("CreateProfile");
        }
    }
}