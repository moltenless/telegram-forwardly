using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class ChatManager
    {
        public static async Task HandleAllChatsToggleAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string input = message.Text!.Trim().ToLower();
            bool? enableAllChats = null;
            if (input == "all") enableAllChats = true;
            else if (input == "next") enableAllChats = false;


            if (enableAllChats is null)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Invalid input. Please send either 'next' or 'all'.",
                    botClient, logger, cancellationToken);
                return;
            }

            FieldUpdateResult result = await userbotApiService.SetAllChatsEnabledAsync(
                user.TelegramUserId, (bool)enableAllChats);

            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                     message.Chat.Id,
                     $"Failed to update all chats setting: {result.ErrorMessage}",
                     botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            await userService.SetUserAllChatsEnabledAsync(user.TelegramUserId, (bool)enableAllChats);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            await RunChatMenuAsync(user, message, userService, userbotApiService,
                botClient, logger, cancellationToken);
        }

        public static async Task RunChatMenuAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            var startPage = GetChatPage(chats, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "view_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "view_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("➕ Add Chat", "add_chat"),
                    InlineKeyboardButton.WithCallbackData("🗑️ Remove Chat", "remove_chat") ],
                [ InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu") ]
            ]);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, "Selected chats:",
                botClient, logger, cancellationToken);
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, startPage, botClient,
                logger, cancellationToken, replyMarkup: keyboard);
        }

        public static async Task HandleViewPageBackAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            var startWith = GetFirstNumberFromPage(callbackQuery.Message!.Text!, logger);
            var page = GetChatPage(chats, startWith!.Value, itemsCount: 10);

        }

        public static string GetChatPage(
            HashSet<Models.Dtos.Chat> chats,
            int startWith, 
            int itemsCount)
        {
            if (chats.Count == 0) return "No chats selected yet.";
            //if (chats.Count < startWith) return "This page is empty.";

            StringBuilder sb = new();
            for (int i = startWith; i < startWith + itemsCount; i++)
            {
                if (i - 1 >= chats.Count) break;

                var chat = chats.ElementAt(i - 1);
                if (chat is null) continue;

                var chatIdWithPrefix = chat.TelegramChatId.ToString();
                var chatId = chatIdWithPrefix.StartsWith("-100")
                    ? chatIdWithPrefix[4..]
                    : chatIdWithPrefix;

                sb.Append($"~~~> *[{chat.Title}](https://t.me/c/{chatId})* < {i} ||-> {chatIdWithPrefix}||\n");
            }

            return sb.ToString();
        }

        private static int? GetFirstNumberFromPage(string message, ILogger logger)
        {
            try
            {
                logger.LogInformation(
                    message);
                return 1;
            }
            catch
            {
                return null;
            }
        }
    }
}
