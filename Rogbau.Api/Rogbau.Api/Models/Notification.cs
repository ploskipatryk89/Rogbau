namespace Rogbau.Api.Models
{
    public class Notification : BaseEntity
    {
        
        public string Message { get; set; } = string.Empty;

        public string? ActionUrl { get; set; }

        public List<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }
}
