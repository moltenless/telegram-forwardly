namespace TelegramForwardly.WebApi.Models.Requests
{
    public class SendMessageRequest
    {
        public long UserId { get; set; }
        public long ForumId { get; set; }
        public long TopicId { get; set; }
        public string TextHeader { get; set; } = null!;
        public string TextFooter { get; set; } = null!;
    }
}
