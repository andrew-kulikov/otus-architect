using Microsoft.AspNetCore.Mvc;

namespace SocialNetwork.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Profiles");

            return View();
        }
    }
}