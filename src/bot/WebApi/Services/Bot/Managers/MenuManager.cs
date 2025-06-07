using Telegram.Bot;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
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

            await BotHelper.SendTextMessageAsync(
                chatId,
                BotHelper.GetSetupMessage(),
                botClient, logger,
                cancellationToken,
                BotHelper.GetPhoneKeyboard());
        }

        public static async Task EnterSettingsAsync(
            BotUser user,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingForumGroup);

            await BotHelper.SendTextMessageAsync(
                chatId, 
                BotHelper.GetSettingsMessage(),
                botClient, logger,
                cancellationToken);
        }
    }
}
