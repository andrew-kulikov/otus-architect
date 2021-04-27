using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Exceptions;
using SocialNetwork.Web.ViewModels;
using IAuthenticationService = SocialNetwork.Core.Services.IAuthenticationService;

namespace SocialNetwork.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var user = await _authenticationService.LoginAsync(model.Username, model.Password);

                return RedirectToAction("Index", "Profiles");
            }
            catch (AuthenticationException e)
            {
                ModelState.AddModelError("", e.Message);

                return View("Index", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}