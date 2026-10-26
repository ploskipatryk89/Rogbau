namespace Rogbau.Api.Models
{
    public class NotificationRecipient : BaseEntity
    {
     
        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}