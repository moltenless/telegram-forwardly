using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public static class CallbackHandler
    {
        public static async Task HandleCallbackAsync(
            BotUser user, 
            string data,
            CallbackQuery callbackQuery,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            switch (data)
            {
                case "setup":
                    await HandleSetupCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
                case "keywords":
                    await HandleKeywordsCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
                case "chats":
                    await HandleChatsCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
                case "status":
                    await HandleStatusCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
                case "settings":
                    await HandleSettingsCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
                case "help":
                    await HandleHelpCallbackAsync(
                        user, callbackQuery,
                        cancellationToken);
                    break;
            }
        }

        private static async Task HandleSetupCallbackAsync(
            BotUser user, 
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }

        private static async Task HandleKeywordsCallbackAsync(
            BotUser user,
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }

        private static async Task HandleChatsCallbackAsync(
            BotUser user,
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }

        private static async Task HandleStatusCallbackAsync(
            BotUser user, 
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }

        private static async Task HandleSettingsCallbackAsync(
            BotUser user,
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }

        private static async Task HandleHelpCallbackAsync(
            BotUser user,
            CallbackQuery callbackQuery, 

            CancellationToken cancellationToken)
        {
        }
    }
}
