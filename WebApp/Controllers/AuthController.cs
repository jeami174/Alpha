using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
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
    public IActionResult SignIn() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInFormModel form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.SignInAsync(form);

        if (result.Succeeded)
            return Json(new { success = true, redirectUrl = result.Result });

        return BadRequest(new { success = false, error = result.Error });
    }

    // ------------------ SIGN UP ------------------
    [HttpGet]
    public IActionResult SignUp() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpFormModel form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.RegisterAsync(form);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                error = result.Error ?? "Registration failed"
            });
        }

        return Json(new { success = true, redirectUrl = result.Result });
    }


    // ------------------ SIGN OUT ------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        await _authService.LogOutAsync();
        return Json(new { success = true, redirectUrl = Url.Action("SignIn", "Auth") });
    }

    // ------------------ FORGOT PASSWORD ------------------

    [HttpGet]
    public IActionResult ForgotPassword() => View();


    // ------------------ ACCESS DENIED ------------------

    [HttpGet("/Auth/Denied")]
    [AllowAnonymous]
    public IActionResult Denied()
    {
        return View();
    }

}

