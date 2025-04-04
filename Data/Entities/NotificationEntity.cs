using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class NotificationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsGlobal { get; set; } = false;

    public string? TargetRole { get; set; }

    public virtual ICollection<UserNotificationEntity> UserNotifications { get; set; } = new List<UserNotificationEntity>();
}
