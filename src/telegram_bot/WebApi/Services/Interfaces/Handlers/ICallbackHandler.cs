using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Interfaces.Handlers
{
    public interface ICallbackHandler
    {
        Task HandleCallbackAsync( 
            BotUser user,
            string data,
            CallbackQuery callbackQuery,
            ICommandHandler commandHandler,
            CancellationToken cancellationToken);
    }
}
