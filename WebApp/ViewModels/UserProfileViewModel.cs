﻿using Domain.Models;

namespace WebApp.ViewModels;

public class UserProfileViewModel
{
    public string FullName { get; set; } = "Unknown";
    public string ImagePath { get; set; } = "/uploads/members/avatars/default.svg";
    public IEnumerable<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();

}
