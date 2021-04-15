using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Web.Controllers
{
    [Authorize]
    public class ProfilesController: Controller
    {
        private readonly IUserRepository _userRepository;

        public ProfilesController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userRepository.GetUserAsync(User.Identity.Name);
            if (user.Profile == null) return RedirectToAction("CreateProfile", "Registration");

            return View();
        }
    }
}
