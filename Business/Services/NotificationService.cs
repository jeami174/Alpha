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
        if (string.IsNullOrWhiteSpace(entity.Image))
        {
            entity.Image = entity.NotificationTypeId switch
            {
                1 => "uploads/members/avatars/default.svg",
                2 => "uploads/projects/avatars/default.svg",
                _ => "uploads/Clients/avatars/default.svg"
            };
        }

        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationModel>> GetNotificationsForUserAsync(string userId, int take = 5)
    {
        var notifications = await _notificationRepository.GetAllForUserAsync(userId, take);

        var models = notifications.Select(n => new NotificationModel
        {
            Id = n.Id,
            Message = n.Message,
            ImagePath = n.Image,
            Created = n.Created
        });

        return models;
    }

    public async Task DismissNotificationAsync(string notificationId, string userId)
    {
        var dismissed = await _context.NotificationDismissed.AnyAsync(x => x.NotificationId == notificationId && x.UserId == userId);
        if (!dismissed)
        {
            var dismissedEntity = new NotificationDismissedEntity
            {
                NotificationId = notificationId,
                UserId = userId,
            };

            _context.Add(dismissedEntity);
            await _context.SaveChangesAsync();
        }
    }
}
