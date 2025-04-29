using System.Security.Claims;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(IHubContext<NotificationHub> notificationHub, INotificationService notificationService) : ControllerBase
{
    private readonly IHubContext<NotificationHub> _notificationHub = notificationHub;
    private readonly INotificationService _notificationService = notificationService;

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> CreateNotification(NotificationEntity entity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Lägg till notification i DB
        await _notificationService.AddNotificationAsync(entity);

        // Läs in NotificationType och TargetGroup
        var notificationType = entity.NotificationType.NotificationType; // Typ: "ProjectCreated", "MemberCreated"
        var targetGroup = entity.NotificationTargetGroup.NotificationTargetGroup; // Grupp: "All", "Admins" osv

        if (notificationType == "ProjectCreated")
        {
            // Ett nytt projekt skapades: skicka till alla
            await _notificationHub.Clients.All.SendAsync("RecieveNotification", new
            {
                Id = entity.Id,
                Message = entity.Message,
                ImagePath = entity.Image,
                Created = entity.Created
            });
        }
        else if (notificationType == "MemberCreated")
        {
            // En ny medlem skapades: skicka endast till admins
            await _notificationHub.Clients.Group("Admins").SendAsync("RecieveNotification", new
            {
                Id = entity.Id,
                Message = entity.Message,
                ImagePath = entity.Image,
                Created = entity.Created
            });
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

        var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
        return Ok(notifications);
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
