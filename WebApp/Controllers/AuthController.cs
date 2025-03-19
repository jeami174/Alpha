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

        public new IActionResult SignOut()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }


    }
}
