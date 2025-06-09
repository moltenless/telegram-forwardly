using System.Threading.Tasks;
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
            await BotHelper.SendTextMessageAsync(
                chatId, BotHelper.GetMenuText(user),
                botClient, logger,
                cancellationToken,
                BotHelper.GetMenuKeyboard());
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
                    "✔️ You're already authenticated! Use /menu\n\n" +
                    "If you want to *remove your data from this bot* " +
                    "including sensitive account information and API credentials:\n" +
                    " - use _*/delete*_\n",
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

        public static async Task AskToDeleteData(
            BotUser user,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingDeleteConfirmation);

            await BotHelper.SendTextMessageAsync(
                chatId,
                "⚠️ Are you sure you want to remove all your data from this bot?\n" +
                "This deletion includes not only secret API credentials but also record of keywords and other settings and preferences.\n" +
                "If you proceed, you will need to set up your account again to be able to use this bot again.\n\n" +
                "Please confirm by sending _'yes, I want this bot not to persist my data'_ or *_'no'_ to cancel*.",
                botClient, logger,
                cancellationToken);
        }

        public static async Task EnterChatsAsync(
            BotUser user, 
            long chatId,
            IUserService userService, 
            ITelegramBotClient botClient,
            ILogger logger, 
            CancellationToken cancellationToken)
        {
            if (!(bool)user.IsAuthenticated!)
            {
                await BotHelper.SendTextMessageAsync(chatId,
                    "⚠️ Setup your credentials first with /setup or via button in menu.\n\n",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingEnableAllChats);
            await BotHelper.SendTextMessageAsync(
                chatId,
                "We recommend you to *select only those chats* where you expect to encounter specific keywords. To do so, *send me _'next'_*.\n\n" +
                "Alternatively, If you want to automatically search and sort messages _from all chats, send me *'all'*_.\n" +
                "However, *last approach is not recommended*, as it may lead to server overload and cause a mess in your forum",
                botClient, logger,
                cancellationToken);
        }
    }
}
