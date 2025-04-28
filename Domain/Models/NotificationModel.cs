namespace Domain.Models
{
    public class NotificationModel
    {
        public string Id { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public DateTime Created { get; set; }
    }
}