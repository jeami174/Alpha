using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;
    private readonly IMemberRepository _memberRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserService userService,
        IMemberRepository memberRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _memberRepository = memberRepository;
    }

    public async Task<ServiceResult<bool>> RegisterAsync(SignUpFormModel form)
    {
        if (form == null)
            return ServiceResult<bool>.Failure("Invalid form data", 400);

        var userResult = await _userService.CreateUserAsync(form.Email, form.Password, form.FirstName, form.LastName, "user");
        if (!userResult.Succeeded)
            return ServiceResult<bool>.Failure(userResult.Error ?? "Could not create user", userResult.StatusCode);

        var user = userResult.Result!;

        var member = new MemberEntity
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            UserId = user.Id
        };

        await _memberRepository.CreateAsync(member);
        await _memberRepository.SaveToDatabaseAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        return ServiceResult<bool>.Success(true, 201);
    }

    public async Task<ServiceResult<bool>> SignInAsync(SignInFormModel form)
    {
        if (form == null)
            return ServiceResult<bool>.Failure("Missing login data", 400);

        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
            return ServiceResult<bool>.Failure("Invalid login attempt", 401);

        var result = await _signInManager.PasswordSignInAsync(user, form.Password, form.RememberMe, false);

        return result.Succeeded
            ? ServiceResult<bool>.Success(true)
            : ServiceResult<bool>.Failure("Invalid credentials", 401);
    }

    public async Task<ServiceResult<bool>> LogOutAsync()
    {
        await _signInManager.SignOutAsync();
        return ServiceResult<bool>.Success(true);
    }
}
