﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.ViewModels;

namespace SocialNetwork.Web.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public RegistrationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
                Username = model.Username
            };

            await _authenticationService.RegisterAsync(user, model.Password);
            await _authenticationService.LoginAsync(model.Username, model.Password);

            return View("CreateProfile");
        }
    }
}