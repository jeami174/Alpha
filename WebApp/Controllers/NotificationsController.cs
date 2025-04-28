using System.Security.Claims;
using Business.Interfaces;
using Data.Entities;
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
    public async Task<IActionResult> CreateNotification(NotificationEntity entity)
    {
        await _notificationService.AddNotificationAsync(entity);

        var notifications = await _notificationService.GetNotificationsForUserAsync("anonymous");

        var newNotification = notifications
            .OrderByDescending(x => x.Created)
            .FirstOrDefault();

        if (newNotification != null)
        {
            await _notificationHub.Clients.All.SendAsync("RecieveNotification", newNotification);
        }

        return Ok(new { success = true });
    }


    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
        return Ok(notifications);
    }

    [HttpPost("dismiss/{id}")]
    public async Task<IActionResult> DismissNotification(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        id = Uri.UnescapeDataString(id);
        await _notificationService.DismissNotificationAsync(id, userId);
        await _notificationHub.Clients.User(userId).SendAsync("NotificationDismissed", id);
        return Ok(new { success = true });
    }

}
