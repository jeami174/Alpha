using Data.Context;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class NotificationRepository(DataContext context) : BaseRepository<NotificationEntity>(context), INotificationRepository
{
    public async Task<IEnumerable<NotificationEntity>> GetAllWithIncludesAsync()
    {
        return await _dbSet
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .Include(x => x.DismissedNotification)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationEntity>> GetAllForUserAsync(string userId, int take = 5)
    {
        var dismissedIds = await _context.NotificationDismissed
            .Where(x => x.UserId == userId)
            .Select(x => x.NotificationId)
            .ToListAsync();

        var notifications = await _dbSet
            .Where(x => !dismissedIds.Contains(x.Id))
            .OrderByDescending(x => x.Created)
            .Take(take)
            .Include(x => x.NotificationType)
            .Include(x => x.NotificationTargetGroup)
            .ToListAsync();

        return notifications;
    }
}
