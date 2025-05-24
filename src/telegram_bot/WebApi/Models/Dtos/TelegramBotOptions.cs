namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class TelegramBotOptions
    {
        public string BotToken { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public bool UseWebhook { get; set; } = false;
        public string UserbotApiBaseUrl { get; set; } = string.Empty;
    }
}
