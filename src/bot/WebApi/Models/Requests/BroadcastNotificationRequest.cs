using System.ComponentModel.DataAnnotations;
using Telegram.Bot.Types.Enums;

namespace TelegramForwardly.WebApi.Models.Requests
{
    public class BroadcastNotificationRequest
    {
        [Required]
        public required string Text { get; set; }
        public bool UseMarkdown { get; set; } = false;
    }
}
