using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public class UserInputHandler : IUserInputHandler
    {
        public async Task HandleUserInputAsync(BotUser user, Message message, CancellationToken cancellationToken)
        {

        }
    }
}
