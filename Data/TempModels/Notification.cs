using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class Notification
{
    public int Id { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsGlobal { get; set; }

    public string? TargetRole { get; set; }

    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
