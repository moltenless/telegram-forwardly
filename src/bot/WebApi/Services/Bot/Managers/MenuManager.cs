using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
                BotHelper.GetMenuKeyboard(user.ForwardlyEnabled!.Value));
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
                BotHelper.GetSetupMessage(user.Phone),
                botClient, logger,
                cancellationToken,
                BotHelper.GetPhoneKeyboard());
        }


        public static async Task EnterSettingsAsync(
            bool editSourceMessage,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("👥 Set group for forwarding", "set_forum")],
                [InlineKeyboardButton.WithCallbackData("🗂️ Set grouping mode", "set_grouping")],
                [InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu")]
            ]);
            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    message.Chat.Id, message.MessageId, "Choose an option:",
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, "Choose an option:", botClient, logger,
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
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
                "Please confirm by sending '`yes, I want this bot not to persist my data`' or *'`no`' to cancel*.",
                botClient, logger,
                cancellationToken);
        }


        public static async Task EnterStatusAsync(
            bool editSourceMessage,
            Message message,
            BotUser user,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string status = user.IsAuthenticated!.Value
                ? "✅ You are authenticated\n"
                : "❌ You are not authenticated - `menu -> setup credentials`\n";
            status += user.ForumSupergroupId is not null
                ? "✅ You've registered group with topics for forwarding messages\n"
                : "❌ You haven't registered group with topics for forwarding messages - `menu -> settings -> 'set group for...`\n";
            if (user.AllChatsFilteringEnabled!.Value)
                status += user.Chats.Count > 0
                    ? "⚠️ You've enabled tracking all the incoming messages, which is not recommended, - `menu -> chats -> track all chats -> next`\n"
                    : "⚠️ You haven't added any particular chats for tracking but enabled tracking all the incoming messages, which is not recommended, - `menu -> chats -> track all chats -> next`\n";
            else
                status += user.Chats.Count > 0
                    ? $"✅ You've added for tracking {user.Chats.Count} chats\n"
                    : "❌ You haven't added any chats for tracking - `menu -> chats -> add`\n";
            status += user.Keywords.Count > 0
                ? $"✅ You've added {user.Keywords.Count} keywords\n"
                : "❌ You haven't added any keywords - `menu -> keywords -> add`\n";
            status += user.ForwardlyEnabled!.Value
                ? "\n🟢 Forwarding is working - not on pause"
                : "\n🔴 Forwarding is on pause";

            var keyboard = new InlineKeyboardMarkup(
            [[InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu")]]);

            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    message.Chat.Id, message.MessageId, BotHelper.DefaultEscapeMarkdownV2(status),
                    replyMarkup: keyboard, parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, status, botClient, logger, cancellationToken, keyboard);
            }
        }

        public static async Task EnterHelpAsync(
           bool editSourceMessage,
           Message message,
           ITelegramBotClient botClient,
           ILogger logger,
           CancellationToken cancellationToken)
        {
            string help = "If you need help you can contact developer @moltenless\n\n" +
                "Available commands:\n" +
                "/menu - Main menu\n" +
                "/setup - Authentication - credentials setup\n" +
                "/settings - Forwarding settings - forum registration and settings grouping mode\n" +
                "/chats - Manage chats to be checked for keywords\n" +
                "/keywords - Manage keywords to be looked for in chats\n" +
                "/status - Check your bot account status\n" +
                "/toggle - Pause or Resume forwarding\n" +
                "/delete - Clear all your account information from bot and remove your sensitive data out of bot database\n" +
                "/help - Get help with bot\n";

            var keyboard = new InlineKeyboardMarkup(
            [[InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu")]]);

            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    message.Chat.Id, message.MessageId, BotHelper.DefaultEscapeMarkdownV2(help),
                    replyMarkup: keyboard, parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, help, botClient, logger, cancellationToken, keyboard);
            }
        }
    }
}