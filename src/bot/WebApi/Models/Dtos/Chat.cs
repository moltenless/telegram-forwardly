namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class Chat
    {
        public long Id { get; set; }
        public long TelegramUserId { get; set; }
        public long TelegramChatId { get; set; }

        public static Chat FromEntity(DataAccess.Entities.Chat entity)
        {
            return new Chat
            {
                Id = entity.Id,
                TelegramUserId = entity.TelegramUserId,
                TelegramChatId = entity.TelegramChatId
            };
        }
    }
}
