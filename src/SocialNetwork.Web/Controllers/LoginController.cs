using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Services;
using SocialNetwork.Web.ViewModels;

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
            var user = await _authenticationService.LoginAsync(model.Username, model.Password);
            return RedirectToAction("Index", "Home");
        }
    }
}