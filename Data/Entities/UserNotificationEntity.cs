using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class UserNotificationEntity
{
    [Key]
    public int Id { get; set; }

    public int NotificationId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    [ForeignKey(nameof(NotificationId))]
    public NotificationEntity Notification { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;
}