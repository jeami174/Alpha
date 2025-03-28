using Business.Models;

namespace Business.Interfaces;

public interface IAuthService
{
    Task<(bool Succeeded, string[] Errors)> RegisterAsync(SignUpFormModel form);
    Task<(bool Succeeded, string? Error)> SignInAsync(SignInFormModel form);
    Task SignOutAsync();
}