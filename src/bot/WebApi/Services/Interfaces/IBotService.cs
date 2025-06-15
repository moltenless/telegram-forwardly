using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Requests;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IBotService
    {
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    }
}
