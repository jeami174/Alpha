using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(object notification)
    {
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationOnlyAdmin(object notification)
    {
        await Clients.Group("Admins").SendAsync("AdminReceiveNotification", notification);
    }
}
