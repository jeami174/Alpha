using Business.Models;
using Data.Entities;
using Domain.Models;

namespace Business.Interfaces;

public interface INotificationService
{
    Task AddNotificationAsync(NotificationEntity entity);
    Task DismissNotificationAsync(string notificationId, string userId);
    Task<IEnumerable<NotificationModel>> GetNotificationsForUserAsync(string userId, int take = 5);
}
