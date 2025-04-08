using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly IFileStorageService _fileStorageService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserService userService,
        IMemberRepository memberRepository,
        ILogger<AuthService> logger,
        IFileStorageService fileStorageService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _memberRepository = memberRepository;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<ServiceResult<string>> RegisterAsync(SignUpFormModel form)
    {
        if (form == null)
            return ServiceResult<string>.Failure("Invalid form data", 400);

        var email = form.Email.Trim().ToLower();
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: email '{Email}' is already in use.", email);
            return ServiceResult<string>.Failure("Email already registered", 409);
        }

        var userResult = await _userService.CreateUserAsync(email, form.Password, form.FirstName, form.LastName, "User");

        if (!userResult.Succeeded)
        {
            _logger.LogError("User creation failed: {Error}", userResult.Error);
            return ServiceResult<string>.Failure(userResult.Error ?? "User creation failed", 500);
        }

        var user = userResult.Result!;

        try
        {
            var randomAvatarPath = _fileStorageService.GetRandomAvatar();

            var member = new MemberEntity
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserId = user.Id,
                ImageName = randomAvatarPath,
                RoleId = null,
                AddressId = null,
                ProjectId = null
            };

            await _memberRepository.CreateAsync(member);
            await _memberRepository.SaveToDatabaseAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);

            return ServiceResult<string>.Success("/projects");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member for user {Email}", email);
            return ServiceResult<string>.Failure("Unexpected error occurred during registration.", 500);
        }
    }

    public async Task<ServiceResult<string>> SignInAsync(SignInFormModel form)
    {
        if (form == null)
            return ServiceResult<string>.Failure("Invalid login data", 400);

        var user = await _userManager.FindByEmailAsync(form.Email.Trim().ToLower());
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found with email {Email}", form.Email);
            return ServiceResult<string>.Failure("Invalid login attempt", 401);
        }

        var result = await _signInManager.PasswordSignInAsync(user, form.Password, form.RememberMe, false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: Incorrect credentials for user {Email}", form.Email);
            return ServiceResult<string>.Failure("Invalid credentials", 401);
        }

        var redirectUrl = "/admin";
        _logger.LogInformation("User {Email} signed in successfully. Redirect to {RedirectUrl}", user.Email, redirectUrl);

        return ServiceResult<string>.Success(redirectUrl);
    }

    public async Task<ServiceResult<bool>> LogOutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out successfully.");
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ServiceResult<string>.Failure("If an account with that email exists, you can reset your password.", 200);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return ServiceResult<string>.Success(token);
    }

    public async Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordFormModel form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
        {
            return ServiceResult<bool>.Failure("User not found", 404);
        }

        var result = await _userManager.ResetPasswordAsync(user, form.Token, form.NewPassword);
        if (result.Succeeded)
        {
            return ServiceResult<bool>.Success(true);
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return ServiceResult<bool>.Failure(errors, 400);
        }
    }
}
