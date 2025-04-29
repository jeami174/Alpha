using Business.Interfaces;
using Business.Models;
using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

public class NotificationService(DataContext context, INotificationRepository notificationRepository) : INotificationService
{
    private readonly DataContext _context = context;
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task AddNotificationAsync(NotificationEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.Image))
        {
            entity.Image = entity.NotificationTypeId switch
            {
                1 => "/uploads/members/avatars/default.svg",   // MemberCreated
                2 => "/uploads/projects/avatars/default.svg",  // ProjectCreated
                _ => "/uploads/clients/avatars/default.svg"    // Fallback
            };
        }

        entity.Created = DateTime.UtcNow;

        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationModel>> GetNotificationsForUserAsync(string userId, int take = 5)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var notifications = await _notificationRepository.GetAllForUserAsync(userId, take);

        return notifications.Select(n => new NotificationModel
        {
            Id = n.Id,
            Message = n.Message,
            ImagePath = n.Image,
            Created = n.Created
        });
    }

    public async Task DismissNotificationAsync(string notificationId, string userId)
    {
        if (string.IsNullOrEmpty(notificationId) || string.IsNullOrEmpty(userId))
            throw new ArgumentException("Notification ID and User ID are required.");

        var alreadyDismissed = await _context.NotificationDismissed
            .AnyAsync(x => x.NotificationId == notificationId && x.UserId == userId);

        if (!alreadyDismissed)
        {
            var dismissedEntity = new NotificationDismissedEntity
            {
                NotificationId = notificationId,
                UserId = userId
            };

            await _context.NotificationDismissed.AddAsync(dismissedEntity);
            await _context.SaveChangesAsync();
        }
    }
}
