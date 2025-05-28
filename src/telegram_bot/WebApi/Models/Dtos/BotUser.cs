using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class BotUser
    {
        public long TelegramUserId { get; set; }
        public UserState CurrentState { get; set; }
        public string? ApiId { get; set; }
        public string? ApiHash { get; set; }
        public string? SessionString { get; set; }
        public string? Phone { get; set; }
        public bool? IsAuthenticated { get; set; }
        public DateTime? RegistrationDataTime { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public long? ForumSupergroupId { get; set; }
        public bool? LoggingTopicEnabled { get; set; }
        public GroupingMode TopicGrouping { get; set; }
        public bool? ForwardlyEnabled { get; set; }
        public bool? AllChatsFilteringEnabled { get; set; }

        public static BotUser FromEntity(Client client)
        {
            return new BotUser
            {
                TelegramUserId = client.TelegramUserId,
                CurrentState = Enum.TryParse(client.CurrentState?.Value, out UserState userState) ? userState : UserState.Idle,
                ApiId = client.ApiId,
                ApiHash = client.ApiHash,
                SessionString = client.SessionString,
                Phone = client.Phone,
                IsAuthenticated = client.IsAuthenticated,
                RegistrationDataTime = client.RegistrationDataTime,
                UserName = client.UserName,
                FirstName = client.FirstName,
                ForumSupergroupId = client.ForumSupergroupId,
                LoggingTopicEnabled = client.LoggingTopicEnabled,
                TopicGrouping = Enum.TryParse(client.TopicGrouping, out GroupingMode groupingMode) ? groupingMode : GroupingMode.ByKeyword,
                ForwardlyEnabled = client.ForwardlyEnabled,
                AllChatsFilteringEnabled = client.AllChatsFilteringEnabled
            };
        }
    }
}
