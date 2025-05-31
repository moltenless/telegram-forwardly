namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class Keyword
    {
        public long Id { get; set; }
        public long TelegramUserId { get; set; }
        public string Value { get; set; } = null!;

        public static Keyword FromEntity(DataAccess.Entities.Keyword entity)
        {
            return new Keyword
            {
                Id = entity.Id,
                TelegramUserId = entity.TelegramUserId,
                Value = entity.Value
            };
        }
    }
}
