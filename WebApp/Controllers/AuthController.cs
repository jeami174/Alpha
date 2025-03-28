using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;


public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    // ------------------ SIGN IN ------------------

    [HttpGet]
    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInFormModel form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.SignInAsync(form);

        if (result.Succeeded)
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });

        return BadRequest(new { success = false, error = result.Error });
    }

    // ------------------ SIGN UP ------------------

    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpFormModel formData)
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

        var result = await _authService.RegisterAsync(formData);

        if (result.Succeeded)
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });

        return BadRequest(new { success = false, errors = result.Errors });
    }

    // ------------------ SIGN OUT ------------------

    [HttpPost]
    public async Task<IActionResult> SignOut()
    {
        await _authService.SignOutAsync();
        return RedirectToAction("SignIn");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }


}
