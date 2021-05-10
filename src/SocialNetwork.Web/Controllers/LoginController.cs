using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Exceptions;
using SocialNetwork.Web.ViewModels;
using IAuthenticationService = SocialNetwork.Core.Services.IAuthenticationService;

namespace SocialNetwork.Web.Controllers
{
    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View("Index", model);
            
            try
            {
                await _authenticationService.LoginAsync(model.Username, model.Password);

                return RedirectToAction("Index", "Profiles");
            }
            catch (AuthenticationException e)
            {
                ModelState.AddModelError("", e.Message);
            }

            return View("Index", model);
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}