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

/// <summary>
/// API controller for managing notifications.
/// Provides endpoints to create notifications, retrieve the current user's notifications,
/// and dismiss individual notifications. Uses SignalR to broadcast real-time updates.
/// I have read the documentation för this setup https://learn.microsoft.com/en-us/aspnet/signalr/
/// </summary>

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

    /// <summary>
    /// Creates a new notification record and broadcasts an update to relevant clients.
    /// - Any authenticated Admin or User may call this.
    /// - Saves the notification via the service layer.
    /// - Broadcasts "NotificationUpdated" to either all clients or only admins,
    ///   depending on the notification type.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> CreateNotification(NotificationEntity entity)
    {
        // Ensure the caller is authenticated
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Persist the notification to the data store
        await _notificationService.AddNotificationAsync(entity);

        // Determine broadcast target based on type
        var notificationType = entity.NotificationType?.NotificationType ?? "";

        if (notificationType == "ProjectCreated")
        {
            // Notify all connected
            await _notificationHub.Clients.All.SendAsync("NotificationUpdated");
        }
        else if (notificationType == "MemberCreated")
        {
            // Notify only the Admins group
            await _notificationHub.Clients.Group("Admins").SendAsync("NotificationUpdated");
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Retrieves up to 5 most recent notifications for the current user,
    /// excluding those already dismissed.
    /// - Admins see all types; regular users see only "ProjectCreated".
    /// - Returns a simplified JSON model with Id, Message, ImagePath, and Created timestamp.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetNotifications()
    {
        // Identify the current user
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        // Fetch IDs of notifications this user has already dismissed
        var dismissedNotificationIds = await _context.NotificationDismissed
            .Where(x => x.UserId == userId)
            .Select(x => x.NotificationId)
            .ToListAsync();
        
        // Build a query for notifications not yet dismissed
        var query = _context.Notifications
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .Where(x => !dismissedNotificationIds.Contains(x.Id));

        if (!isAdmin)
        {
            // Filter out member-related notifications for non-admins
            query = query.Where(x => x.NotificationType.NotificationType == "ProjectCreated");
        }
        // Return the top 5 most recent
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

    /// <summary>
    /// Marks a specific notification as dismissed for the current user
    /// and notifies the client in real time to remove it from the UI.
    /// </summary>
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
