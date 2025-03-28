
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(SignUpFormModel form)
    {
        var user = new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            FirstName = form.FirstName,
            LastName = form.LastName
        };

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

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

}