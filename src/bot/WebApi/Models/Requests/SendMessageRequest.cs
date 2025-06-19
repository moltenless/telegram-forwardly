namespace TelegramForwardly.WebApi.Models.Requests
{
    public class SendMessageRequest
    {
        public long ForumOwnerId { get; set; }
        public long ForumId { get; set; }
        public long TopicId { get; set; }
        public string SourceText { get; set; } = null!;
        public long SourceMessageId { get; set; }
        public long SourceChatId { get; set; }
        public string SourceChatTitle { get; set; } = null!;
        public string[] FoundKeywords { get; set; } = [];
        public long SenderId { get; set; }
        public string? SenderFirstName { get; set; }
        public string? SenderUsername { get; set; }
        public string DateTime { get; set; } = null!;
    }
}
