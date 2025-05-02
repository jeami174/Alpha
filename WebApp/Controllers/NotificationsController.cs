using System.Security.Claims;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(
    IHubContext<NotificationHub> notificationHub,
    INotificationService notificationService,
    UserManager<ApplicationUser> userManager,
    DataContext context) : ControllerBase
{
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly INotificationService _notificationService = notificationService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly DataContext _context = context;

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> CreateNotification(NotificationEntity entity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _notificationService.AddNotificationAsync(entity);

        var notificationType = entity.NotificationType?.NotificationType ?? "";

        if (notificationType == "ProjectCreated")
        {
            await _notificationHub.Clients.All.SendAsync("NotificationUpdated");
        }
        else if (notificationType == "MemberCreated")
        {
            await _notificationHub.Clients.Group("Admins").SendAsync("NotificationUpdated");
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var dismissedNotificationIds = await _context.NotificationDismissed
            .Where(x => x.UserId == userId)
            .Select(x => x.NotificationId)
            .ToListAsync();

        var query = _context.Notifications
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .Where(x => !dismissedNotificationIds.Contains(x.Id));

        if (!isAdmin)
        {
            // Om user inte är admin → visa endast "ProjectCreated"
            query = query.Where(x => x.NotificationType.NotificationType == "ProjectCreated");
        }

        var notifications = await query
            .OrderByDescending(x => x.Created)
            .Take(5)
            .ToListAsync();

        var models = notifications.Select(x => new
        {
            Id = x.Id,
            Message = x.Message,
            ImagePath = x.Image,
            Created = x.Created
        });

        return Ok(models);
    }

    [HttpPost("dismiss/{id}")]
    [Authorize]
    public async Task<IActionResult> DismissNotification(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        id = Uri.UnescapeDataString(id);

        await _notificationService.DismissNotificationAsync(id, userId);
        await _notificationHub.Clients.User(userId).SendAsync("NotificationDismissed", id);

        return Ok(new { success = true });
    }
}
