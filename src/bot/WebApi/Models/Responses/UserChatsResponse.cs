namespace TelegramForwardly.WebApi.Models.Responses
{
    public class UserChatsResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<ChatInfo> Chats { get; set; } = [];
    }
      
    public class ChatInfo
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
    }
}
