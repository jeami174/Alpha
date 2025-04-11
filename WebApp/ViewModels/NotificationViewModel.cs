namespace WebApp.ViewModels;

public class NotificationViewModel
{
    public string ImagePath { get; set; } = "/uploads/members/avatars/default.svg"; ///OBS Kom ihåg att ändra, bara dummy behöver ändras senare för att visa rätt bild med logik.
    public string Message { get; set; } = "New notification"; //OBS Dummy
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<NotificationViewModel> Notifications { get; set; } = [];

}
