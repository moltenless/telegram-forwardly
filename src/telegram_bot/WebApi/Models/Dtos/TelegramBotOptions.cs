namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class TelegramBotOptions
    {
        public string BotToken { get; set; } = null!;
        public bool? UseWebhook { get; set; } = null!;
        public string WebhookUrl { get; set; } = null!;
        public string UserbotApiBaseUrl { get; set; } = null!;
    }
}
