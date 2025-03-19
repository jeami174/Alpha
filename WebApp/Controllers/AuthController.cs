using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{

    public class AuthController : Controller
    {

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string test)
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }


        [HttpPost]
        public IActionResult SignUp(SignUpFormModel formData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                    );

                return BadRequest(new { success = false, errors });
            }

            //var result = await _clientService.AddClientAsync(form);
            //if (result)
            //{
            //    return Ok(new { success = true });
            //}
            //else
            //{
            // return Problem("unable to submit data")
            //}

            return Ok(new { success = true }); //radera senare
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
