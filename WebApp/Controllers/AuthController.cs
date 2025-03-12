using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{

    public class AuthController : Controller
    {
        public IActionResult SignIn()
        {
            return View();
        }

        [Route("signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("signout")]
        public new IActionResult SignOut()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
