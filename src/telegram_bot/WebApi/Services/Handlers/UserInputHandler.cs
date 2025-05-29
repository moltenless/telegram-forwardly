using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public static class UserInputHandler
    {
        public static async Task HandleUserInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {

        }
    }
}
