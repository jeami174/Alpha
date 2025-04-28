using System.Security.Claims;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;

namespace WebApp.Controllers;

public class AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, INotificationService notificationService, IHubContext<NotificationHub> notificationHub, IProjectService projectService, IMemberService memberService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IProjectService _projectService = projectService;
    private readonly IMemberService _memberService = memberService;


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
        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { success = false, errors });
        }

        var result = await _authService.SignInAsync(form);

        if (result.Succeeded)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

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

                // Uppdatera LastLogin till nu!
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            return Json(new { success = true, redirectUrl = result.Result });
        }

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
        return Json(new { success = true, redirectUrl = Url.Action("ResetPassword", new { email = model.Email, token }) });
    }


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

}

