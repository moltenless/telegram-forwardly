namespace TelegramForwardly.WebApi.Models.Requests
{
    public class SendMessageRequest
    {
        public long UserId { get; set; }
        public long ForumId { get; set; }
        public long TopicId { get; set; }
        public string Message { get; set; } = null!;
    }
}
