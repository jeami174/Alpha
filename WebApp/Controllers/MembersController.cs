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

[Authorize(Policy = "Admins")]
public class MembersController(
    IMemberService memberService,
    IFileStorageService fileStorageService,
    INotificationService notificationService,
    IHubContext<NotificationHub> notificationHub,
    UserManager<ApplicationUser> userManager
) : Controller
{
    private readonly IMemberService _memberService = memberService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private const string UserUploadsFolder = "members/useruploads";
    private const string AvatarsFolder = "members/avatars";

    public async Task<IActionResult> Index()
    {
        var result = await _memberService.GetAllMembersAsync();
        return View(result.Succeeded ? result.Result : new List<MemberModel>());
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(AddMemberForm form)
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

        string imageName = form.MemberImage is { Length: > 0 }
            ? await _fileStorageService.SaveFileAsync(form.MemberImage, UserUploadsFolder)
            : _fileStorageService.GetRandomAvatar(AvatarsFolder);

        var result = await _memberService.AddMemberAsync(form, imageName);
        if (!result.Succeeded || result.Result == null)
            return BadRequest(new { success = false, error = result.Error ?? "Failed to create member." });

        var notification = new NotificationEntity
        {
            Message = $"New member: {result.Result.FirstName} {result.Result.LastName}",
            Image = imageName,
            NotificationTypeId = 1,
            NotificationTargetGroupId = 1,
            Created = DateTime.UtcNow
        };

        await _notificationService.AddNotificationAsync(notification);

        await _notificationHub.Clients.Group("Admins").SendAsync("NotificationUpdated");

            return Ok(new { success = true });
    }


    [HttpGet]
    public async Task<IActionResult> EditMember(int id)
    {
        var result = await _memberService.GetMemberByIdAsync(id);
        if (!result.Succeeded || result.Result == null)
            return NotFound();

        var member = result.Result;
        var form = new EditMemberForm
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            MemberEmail = member.Email,
            Phone = member.Phone,
            DateOfBirth = member.DateOfBirth,
            ImageName = member.ImageName,
            RoleName = member.Role?.Name,
            Street = member.Address?.Street,
            PostalCode = member.Address?.PostalCode,
            City = member.Address?.City
        };

        return PartialView("~/Views/Shared/Partials/Sections/_EditMemberForm.cshtml", form);
    }

    [HttpPost]
    public async Task<IActionResult> EditMember(EditMemberForm form)
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

        if (form.MemberImage is { Length: > 0 })
        {
            string newImageName = await _fileStorageService.SaveFileAsync(form.MemberImage, UserUploadsFolder);
            form.ImageName = newImageName;
        }

        var result = await _memberService.UpdateMemberAsync(form.Id, form);
        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var result = await _memberService.DeleteMemberAsync(id);
        return result.Succeeded
            ? Ok(new { success = true })
            : BadRequest(new { success = false, error = result.Error });
    }
}
