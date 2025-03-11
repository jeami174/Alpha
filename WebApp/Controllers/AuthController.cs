using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        [Route("sign-up")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("signin")]
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
