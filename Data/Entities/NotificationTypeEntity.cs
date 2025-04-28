using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class NotificationTypeEntity
{
    [Key]
    public int Id { get; set; }
    public string NotificationType { get; set; } = null!;
    public ICollection<NotificationEntity> Notifications { get; set; } = [];
}
