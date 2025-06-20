namespace TelegramForwardly.WebApi.Models.Dtos
{
    public class TelegramConfig
    {
        public string BotToken { get; set; } = null!;
        public bool? UseWebhook { get; set; } = null!;
        public string WebhookUrl { get; set; } = null!;
        public string UserbotApiBaseUrl { get; set; } = null!;
        public string AuthApiBaseUrl { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string WebhookSecretToken { get; set; } = null!;
    }
}
