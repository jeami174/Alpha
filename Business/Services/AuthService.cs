
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    UserFactory userFactory) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserFactory _userFactory = userFactory;

    public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(SignUpFormModel form)
    {
        var existingUser = await _userManager.FindByEmailAsync(form.Email);
        if (existingUser != null)
        {
            return (false, new[] { "An account with this email already exists." });
        }

        var user = _userFactory.Create(form);
        var result = await _userManager.CreateAsync(user, form.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return (true, []);
        }

        return (false, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string? Error)> SignInAsync(SignInFormModel form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email);

        if (user == null)
            return (false, "Invalid login attempt");

        var result = await _signInManager.PasswordSignInAsync(user, form.Password, form.RememberMe, false);

        return result.Succeeded
            ? (true, null)
            : (false, "Invalid login credentials");
    }

    public async Task LogOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

}