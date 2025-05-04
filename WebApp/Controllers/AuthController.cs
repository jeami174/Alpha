using System.Security.Claims;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;

namespace WebApp.Controllers;

/// <summary>
/// Handles all authentication flows: local sign-in/up/out, password reset,
/// external (OAuth) sign-in, and notifications via SignalR.
/// </summary>
public class AuthController(IMemberRepository memberRepository, IFileStorageService fileStorageService, IAuthService authService, UserManager<ApplicationUser> userManager, INotificationService notificationService, IHubContext<NotificationHub> notificationHub, IProjectService projectService, IMemberService memberService, SignInManager<ApplicationUser> signInManager) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IProjectService _projectService = projectService;
    private readonly IMemberService _memberService = memberService;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    #region Local Identity
    
    // ------------------ SIGN IN ------------------

    [HttpGet]
    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInFormModel form)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { success = false, errors });
        }

        // Attempt to sign in via business layer
        var result = await _authService.SignInAsync(form);

        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, error = result.Error });
        }

        // Fetch user and enforce authorization
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null)
        {
            return Unauthorized();
        }

        var userId = user.Id;

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        // Send real-time notifications for newly created projects/members
        try
        {
            // Admins see both new projects and new members
            if (isAdmin)
            {
                var newProjects = await _projectService.GetNewProjectsAsync(user.LastLogin);
                var newMembers = await _memberService.GetNewMembersAsync(user.LastLogin);

                foreach (var project in newProjects)
                {
                    var notification = new NotificationModel
                    {
                        Id = project.Id,
                        Message = $"New project: {project.ProjectName}",
                        ImagePath = project.ImageName ?? "/uploads/projects/avatars/default.svg",
                        Created = project.Created
                    };

                    await _notificationHub.Clients.User(userId).SendAsync("RecieveNotification", notification);
                }

                foreach (var member in newMembers)
                {
                    var notification = new NotificationModel
                    {
                        Id = member.Id.ToString(),
                        Message = $"New member: {member.FirstName} {member.LastName}",
                        ImagePath = member.ImageName ?? "/uploads/members/avatars/default.svg",
                        Created = member.Created
                    };

                    await _notificationHub.Clients.User(userId).SendAsync("RecieveNotification", notification);
                }
            }
            // Regular users see only new projects
            else
            {
                var newProjects = await _projectService.GetNewProjectsAsync(user.LastLogin);

                foreach (var project in newProjects)
                {
                    var notification = new NotificationModel
                    {
                        Id = project.Id,
                        Message = $"New project: {project.ProjectName}",
                        ImagePath = project.ImageName ?? "/uploads/projects/avatars/default.svg",
                        Created = project.Created
                    };

                    await _notificationHub.Clients.User(userId).SendAsync("RecieveNotification", notification);
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Logga exception om  ILogger
            // Men låt inte en misslyckad SignalR-sändning förstöra inloggningen.
        }

        // Update the user's last login timestamp
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Return JSON with redirect URL
        return Json(new { success = true, redirectUrl = result.Result });
    }

    // ------------------ SIGN UP ------------------
    [HttpGet]
    public IActionResult SignUp() => View();

    /// <summary>
    /// Processes sign-up form via AJAX:
    /// - Validates model state
    /// - Calls AuthService.RegisterAsync
    /// - Returns redirect URL on success
    /// </summary>
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

    /// <summary>
    /// Handles forgot password requests via AJAX:
    /// - Validates model state
    /// - Generates reset token if user exists
    /// - Returns URL for reset page
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordFormModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("Email", "That email address is not registered.");
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.GeneratePasswordResetTokenAsync(model.Email);
        string token = result.Succeeded ? (result.Result ?? "") : "";
        // Redirect to ResetPassword action with email and token
        return Json(new { success = true, redirectUrl = Url.Action("ResetPassword", new { email = model.Email, token }) });
    }

    /// <summary>
    /// Displays the reset password page with email and token populated.
    /// </summary>
    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        var model = new ResetPasswordFormModel
        {
            Email = email,
            Token = token
        };
        return View(model);
    }

    /// <summary>
    /// Processes reset password form via AJAX:
    /// - Validates model state
    /// - Calls AuthService.ResetPasswordAsync
    /// - Returns JSON success or validation errors
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordFormModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { success = false, errors });
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("Email", "Could not find user with that email address.");
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.ResetPasswordAsync(model);
        if (result.Succeeded)
        {
            return Json(new { success = true, redirectUrl = Url.Action("SignIn", "Auth") });
        }

        ModelState.AddModelError("NewPassword", result.Error ?? "Failed to reset password");
        var allErrors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        return BadRequest(new { success = false, errors = allErrors });
    }

    // ------------------ ACCESS DENIED ------------------

    [HttpGet("/Auth/Denied")]
    [AllowAnonymous]
    public IActionResult Denied()
    {
        return View();
    }

    // ------------------ ADMIN SIGN IN ------------------
    /// <summary>
    /// Displays the admin-only sign-in page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AdminLogin()
    {
        return View();
    }

    /// <summary>
    /// Processes admin login via AJAX:
    /// - Validates input
    /// - Ensures user has Admin role
    /// - Updates LastLogin and returns redirect URL
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(SignInFormModel form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.SignInAsync(form);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, error = result.Error });
        }

        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null || !(await _userManager.IsInRoleAsync(user, "Admin")))
        {
            return BadRequest(new
            {
                success = false,
                error = "You must be an administrator to log in here.",
                errors = new Dictionary<string, string[]>
            {
                { "", new[] { "You must be an administrator to log in here." } }
            }
            });
        }

        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Json(new { success = true, redirectUrl = result.Result });
    }



    #endregion

    #region External Authentication

    /// <summary>
    /// Initiates external login (e.g. Google, Facebook) by issuing a challenge.
    /// This is from Hans video.
    /// </summary>

    [HttpGet, HttpPost]
    public IActionResult ExternalSignIn(string provider, string returnUrl = null!)
    {
        if (string.IsNullOrEmpty(provider))
        {
            ModelState.AddModelError("", "Invalid provider");
            return View("SignIn");
        }

        var redirectUrl = Url.Action("ExternalSignInCallback", "Auth", new { returnUrl })!;
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalSignInCallback(string returnUrl = null!, string remoteError = null!)
    {
        returnUrl ??= Url.Content("~/");

        if (!string.IsNullOrEmpty(remoteError))
        {
            ModelState.AddModelError("", $"Error from external provider: {remoteError}");
            return View("SignIn");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return RedirectToAction("SignIn");

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            string firstName = string.Empty;
            string lastName = string.Empty;

            try
            {
                firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "unknown";
                lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? firstName;
            }
            catch { }

            string email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            string userName = $"ext_{info.LoginProvider.ToLower()}_{email}";

            var user = new ApplicationUser
            {
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            var identityResult = await _userManager.CreateAsync(user);
            if (identityResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);

                var member = new MemberEntity
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    UserId = user.Id,
                    ImageName = _fileStorageService.GetRandomAvatar("members/avatars"),
                    RoleId = null,
                    AddressId = null
                };

                await _memberRepository.CreateAsync(member);
                await _memberRepository.SaveToDatabaseAsync();

                return LocalRedirect(returnUrl);
            }

            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("SignIn");
        }
    }

    #endregion

}

