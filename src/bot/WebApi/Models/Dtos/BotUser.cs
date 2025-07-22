using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class BotUser
    {
        public long TelegramUserId { get; set; }
        public UserState? CurrentState { get; set; }
        public string? ApiId { get; set; }
        public string? ApiHash { get; set; }
        public string? SessionString { get; set; }
        public string? Phone { get; set; }
        public bool? IsAuthenticated { get; set; }
        public DateTime? RegistrationDateTime { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public long? ForumSupergroupId { get; set; }
        public GroupingMode? TopicGrouping { get; set; }
        public bool? ForwardlyEnabled { get; set; }
        public bool? AllChatsFilteringEnabled { get; set; }
        public int? ThresholdCharsCount { get; set; }

        public ICollection<Keyword> Keywords { get; set; } = new HashSet<Keyword>();
        public ICollection<Chat> Chats { get; set; } = new HashSet<Chat>();

        public static BotUser FromEntity(Client client)
        {
            var keywords = client.Keywords.Select(k => new Keyword
            {
                Id = k.Id,
                TelegramUserId = k.TelegramUserId,
                Value = k.Value
            }).ToHashSet() ?? [];

            var chats = client.Chats.Select(c => new Chat
            {
                Id = c.Id,
                TelegramUserId = c.TelegramUserId,
                TelegramChatId = c.TelegramChatId,
            }).ToHashSet() ?? [];

            return new BotUser
            {
                TelegramUserId = client.TelegramUserId,
                CurrentState = Enum.TryParse(client.CurrentState?.Value, out UserState userState) ? userState : null,
                ApiId = client.ApiId,
                ApiHash = client.ApiHash,
                SessionString = client.SessionString,
                Phone = client.Phone,
                IsAuthenticated = client.IsAuthenticated,
                RegistrationDateTime = client.RegistrationDataTime,
                UserName = client.UserName,
                FirstName = client.FirstName,
                ForumSupergroupId = client.ForumSupergroupId,
                TopicGrouping = Enum.TryParse(client.TopicGrouping, out GroupingMode groupingMode) ? groupingMode : null,
                ForwardlyEnabled = client.ForwardlyEnabled,
                AllChatsFilteringEnabled = client.AllChatsFilteringEnabled,
                ThresholdCharsCount = client.ThresholdCharsCount,
                Keywords = keywords,
                Chats = chats
            };
        }
    }
}
