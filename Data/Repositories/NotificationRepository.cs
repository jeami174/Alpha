using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Repository for accessing notifications, including methods
/// to retrieve all notifications with related data and to fetch
/// user-specific, non-dismissed notifications.
/// </summary>
public class NotificationRepository(DataContext context) : BaseRepository<NotificationEntity>(context), INotificationRepository
{
    /// <summary>
    /// Retrieves all notifications along with their NotificationType,
    /// NotificationTargetGroup, and any dismissed records.
    /// </summary>
    public async Task<IEnumerable<NotificationEntity>> GetAllWithIncludesAsync()
    {
        return await _dbSet
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .Include(x => x.DismissedNotification)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the most recent notifications for the specified user
    /// that have not been dismissed, ordered by creation date and
    /// limited to the given count.
    /// </summary>
    public async Task<IEnumerable<NotificationEntity>> GetAllForUserAsync(string userId, int take = 5)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var notifications = await _dbSet
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .Where(x => !_context.NotificationDismissed
                .Where(d => d.UserId == userId)
                .Select(d => d.NotificationId)
                .Contains(x.Id))
            .OrderByDescending(x => x.Created)
            .Take(take)
            .ToListAsync();

        return notifications;
    }
}
