using Data.Entities;

namespace Data.Interfaces
{
    public interface INotificationRepository : IBaseRepository<NotificationEntity>
    {
        Task<IEnumerable<NotificationEntity>> GetAllWithIncludesAsync();
        Task<IEnumerable<NotificationEntity>> GetAllForUserAsync(string userId, int take = 5);
    }
}
