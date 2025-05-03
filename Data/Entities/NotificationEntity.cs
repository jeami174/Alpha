using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class NotificationEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Image { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.Now;

    [ForeignKey(nameof(NotificationTargetGroup))]
    public int NotificationTargetGroupId { get; set; } = 1;
    public NotificationTargetGroupEntity NotificationTargetGroup { get; set; } = null!;

    [ForeignKey(nameof(NotificationType))]
    public int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    public ICollection<NotificationDismissedEntity> DismissedNotification { get; set; } = [];
}
