using Telegram.Bot.Types;

namespace TelegramForwardly.WebApi.Services.Interfaces
{
    public interface IBotService
    {
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    }
}
