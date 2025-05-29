using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public class CallbackHandler : ICallbackHandler
    {
        public async Task HandleCallbackAsync(BotUser user, string data,
            CallbackQuery callbackQuery,
            ICommandHandler commandHandler,
            CancellationToken cancellationToken)
        {
            switch (data)
            {
                case "setup":
                    await HandleSetupCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
                case "keywords":
                    await HandleKeywordsCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
                case "chats":
                    await HandleChatsCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
                case "status":
                    await HandleStatusCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
                case "settings":
                    await HandleSettingsCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
                case "help":
                    await HandleHelpCallbackAsync(user, callbackQuery,
                        commandHandler, cancellationToken);
                    break;
            }
        }

        private async Task HandleSetupCallbackAsync(BotUser user, 
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }

        private async Task HandleKeywordsCallbackAsync(BotUser user,
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }

        private async Task HandleChatsCallbackAsync(BotUser user,
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }

        private async Task HandleStatusCallbackAsync(BotUser user, 
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }

        private async Task HandleSettingsCallbackAsync(BotUser user,
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }

        private async Task HandleHelpCallbackAsync(BotUser user,
            CallbackQuery callbackQuery, 
            ICommandHandler commandHandler, CancellationToken cancellationToken)
        {
        }
    }
}
