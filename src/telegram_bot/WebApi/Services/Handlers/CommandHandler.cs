using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public class CommandHandler : ICommandHandler
    {
        public async Task HandleCommandAsync(BotUser user, Message message, CancellationToken cancellationToken)
        {
            var command = message.Text!.Split(' ')[0].ToLower();

            switch()
        }
    }
}
