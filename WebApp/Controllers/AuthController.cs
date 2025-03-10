using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        [Route("sign-up")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("sign-in")]
        public IActionResult SignIn()
        {
            return View();
        }

        public new IActionResult SignOut()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
