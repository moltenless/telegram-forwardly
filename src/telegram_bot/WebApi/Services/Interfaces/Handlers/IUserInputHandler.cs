using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Interfaces.Handlers
{
    public interface IUserInputHandler
    {
        Task HandleUserInputAsync(BotUser user, Message message, CancellationToken cancellationToken);
    }
}
