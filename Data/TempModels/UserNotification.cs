using System;
using System.Collections.Generic;

namespace Data.TempModels;

public partial class UserNotification
{
    public int Id { get; set; }

    public int NotificationId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
