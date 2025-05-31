using Telegram.Bot;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot
{
    public static class MenuManager
    {
        public static async Task ShowMainMenuAsync(
            BotUser user,
            long chatId,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keyboard = BotHelper.GetMenuKeyboard();

            var menuText = (bool)user.IsAuthenticated!
                ? "🏠 Main Menu - You're authenticated and ready to go!"
                : "🏠 Main Menu - Please set up your credentials first.";

            await BotHelper.SendTextMessageAsync(
                chatId, menuText,
                botClient, logger,
                cancellationToken,
                keyboard);
        }

        public static async Task EnterSetupAsync(
            BotUser user,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if ((bool)user.IsAuthenticated!)
            {
                await BotHelper.SendTextMessageAsync(chatId,
                    "You're already authenticated! Use /menu to see available options.",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingPhoneNumber);

            var setupMessage = BotHelper.GetSetupMessage();
            var keyboard = BotHelper.GetPhoneKeyboard();

            await BotHelper.SendTextMessageAsync(
                chatId,
                setupMessage,
                botClient, logger,
                cancellationToken,
                keyboard);
        }
    }
}
