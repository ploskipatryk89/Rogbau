namespace Rogbau.Api.Features.Notifications.Dtos
{
    public record NotificationDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime DateOfNotification { get; set; }
    }
}
