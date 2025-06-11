using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class ChatManager
    {
        public static async Task RunChatMenuAsync(
            bool editSourceMessage,
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (!(bool)user.IsAuthenticated!)
            {
                await BotHelper.SendTextMessageAsync(message.Chat.Id,
                    "⚠️ Setup your credentials first with /setup or via button in menu.\n\n",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });
            var startPage = GetChatPage(chatInfos, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "chat_view_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "chat_view_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("➕ Add Chat", "add_chat"),
                    InlineKeyboardButton.WithCallbackData("🗑️ Remove Chat", "remove_chat") ],
                [ InlineKeyboardButton.WithCallbackData("🌐 Track All Chats", "enable_all_chats"),
                    InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu")]
            ]);

            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    message.Chat.Id, 
                    message.MessageId,
                    BotHelper.DefaultEscapeMarkdownV2(startPage),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, startPage,
                    botClient, logger, cancellationToken,
                    replyMarkup: keyboard);
            }
        }

        public static async Task AskToEnableAllChatsAsync(
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingEnableAllChats);
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "We recommend you to *select only those chats* where you expect to encounter specific keywords. To do so, *send me _'next'_*.\n\n" +
                "Alternatively, If you want to automatically search and sort messages _from all chats, send me *'all'*_.\n" +
                "However, *last approach is not recommended*, as it may lead to server overload and cause a mess in your forum",
                botClient, logger,
                cancellationToken);
        }

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

            await RunChatMenuAsync(false, user, message, userService, userbotApiService,
                botClient, logger, cancellationToken);
        }

        public static async Task StartChatAdditionAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            UserChatsResponse response = await userbotApiService.GetUserChatsAsync(user.TelegramUserId);
            if (!response.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    callbackQuery.Message!.Chat.Id,
                    $"Failed to retrieve user chats: {response.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            List<ChatInfo> chatInfos = response.Chats;//////    SHOW ONLY NOT ADDED CHATS
            string page = GetChatPage(chatInfos, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "chat_addition_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "chat_addition_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("💬 Back to Chat Menu", "back_to_chat_menu")]]);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            await BotHelper.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                "*Select chats above* ⬆️. Send me id of chosen ones.\nTap to copy it. " +
                "To choose *many chats*, delimit them with space\n\nFor example: `123456789 987654321`",
                botClient, logger, cancellationToken);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingChats);
        }

        public static async Task TurnViewPageBackAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });

            var oldPageFirstNumber = ParseFirstNumber(callbackQuery.Message!.Text!);
            if (oldPageFirstNumber is null || oldPageFirstNumber <= 1)
            {
                return;
            }

            int startWith;
            if (oldPageFirstNumber <= 10)
            {
                startWith = 1;
            }
            else
            {
                startWith = oldPageFirstNumber.Value - 10;
            }

            var page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static async Task TurnViewPageForwardAsync(
           BotUser user,
           CallbackQuery callbackQuery,
           IUserService userService,
           ITelegramBotClient botClient,
           ILogger logger,
           CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });

            int? oldPageLastNumber = ParseLastNumber(callbackQuery.Message!.Text!);
            if (oldPageLastNumber is null || oldPageLastNumber >= chats.Count)
            {
                return;
            }

            int startWith = oldPageLastNumber.Value + 1;
            var page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static string GetChatPage(
            IEnumerable<ChatInfo> chats,
            int startWith,
            int itemsCount)
        {
            if (!chats.Any()) return "No chats selected yet.";

            StringBuilder sb = new();
            for (int i = startWith; i < startWith + itemsCount; i++)
            {
                if (i - 1 >= chats.Count()) break;

                var chat = chats.ElementAt(i - 1);
                if (chat is null) continue;

                var chatIdWithPrefix = chat.Id.ToString();
                var chatId = chatIdWithPrefix.StartsWith("-100")
                    ? chatIdWithPrefix[4..]
                    : chatIdWithPrefix;

                sb.Append($"> *[{BotHelper.RemoveSpecialChars(chat.Title)}](https://t.me/c/{chatId}/1000000)*\n" +
                    $"{i}   id: `{chatIdWithPrefix} `\n");
            }

            return sb.ToString();
        }

        private static int? ParseFirstNumber(string page)
        {
            try
            {
                string[] lines = page.Split(">",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string[] splitFirstLine = lines[0].Split(["\n", "id:"],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                return int.Parse(splitFirstLine[1].Trim());
            }
            catch
            {
                return null;
            }
        }

        private static int? ParseLastNumber(string page)
        {
            try
            {
                string[] lines = page.Split(">",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string[] splitLastLine = lines[^1].Split(["\n", "id:"],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return int.Parse(splitLastLine[1].Trim());
            }
            catch
            {
                return null;
            }
        }

        public static async Task TurnAddPageBackAsync(BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            UserChatsResponse response = await userbotApiService.GetUserChatsAsync(user.TelegramUserId);
            if (!response.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    callbackQuery.Message!.Chat.Id,
                    $"Failed to retrieve user chats: {response.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            List<ChatInfo> chatInfos = response.Chats;//////    SHOW ONLY NOT ADDED CHATS

            var oldPageFirstNumber = ParseFirstNumber(callbackQuery.Message!.Text!);
            if (oldPageFirstNumber is null || oldPageFirstNumber <= 1)
            {
                return;
            }

            int startWith;
            if (oldPageFirstNumber <= 10)
            {
                startWith = 1;
            }
            else
            {
                startWith = oldPageFirstNumber.Value - 10;
            }

            string page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static async Task TurnAddPageForwardAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            UserChatsResponse response = await userbotApiService.GetUserChatsAsync(user.TelegramUserId);
            if (!response.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    callbackQuery.Message!.Chat.Id,
                    $"Failed to retrieve user chats: {response.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            List<ChatInfo> chatInfos = response.Chats;//////    SHOW ONLY NOT ADDED CHATS

            int? oldPageLastNumber = ParseLastNumber(callbackQuery.Message!.Text!);
            if (oldPageLastNumber is null || oldPageLastNumber >= chatInfos.Count)
            {
                return;
            }

            int startWith = oldPageLastNumber.Value + 1;
            var page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static async Task HandleChatSelectionAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string[] chatIds = message.Text!.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (chatIds.Length == 0)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "No chat IDs provided. Please send at least one ID.",
                    botClient, logger, cancellationToken);
                return;
            }
            List<long> addedChats = [];
            foreach (string chatId in chatIds)
            {
                if (long.TryParse(chatId, out long parsedChatId))
                {
                    addedChats.Add(parsedChatId);
                }
                else
                {
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        $"Invalid chat ID: {chatId}. Please provide valid numeric IDs.",
                        botClient, logger, cancellationToken);
                }
            }
            if (addedChats.Count == 0)
            {
                return;
            }
            UserChatsResponse result = await userbotApiService.AddChatsAsync(user.TelegramUserId, addedChats);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to add chats: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }
            await userService.AddUserChatsAsync(user.TelegramUserId, result.Chats);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
            user = await userService.GetUserAsync(user.TelegramUserId);

            if (user.Keywords.Count == 0)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Chats successfully added!\n\n" +
                    "Now, please add some keywords to filter messages in chats.\n" +
                    "*Please click '📝 Keywords' in menu below*\nOr\n*Use /keywords*",
                    botClient, logger, cancellationToken);
                await MenuManager.ShowMainMenuAsync(
                    user, message.Chat.Id,
                    botClient, logger, cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Chats successfully added!",
                    botClient, logger, cancellationToken);
                await RunChatMenuAsync(false, user, message, userService, userbotApiService,
                    botClient, logger, cancellationToken);
            }
        }


        public static async Task StartChatDeletionAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });
            var startPage = GetChatPage(chatInfos, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "chat_deletion_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "chat_deletion_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("💬 Back to Chat Menu", "back_to_chat_menu")]]);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(startPage),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            await BotHelper.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                "*Select chats to STOP TRACKING above* ⬆️. Send me id of chosen ones.\nTap to copy it. " +
                "To choose *many chats*, delimit them with space\n\nFor example: `123456789 987654321`",
                botClient, logger, cancellationToken);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingRemoveChats);
        }

        public static async Task TurnDeletePageBackAsync(BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });

            var oldPageFirstNumber = ParseFirstNumber(callbackQuery.Message!.Text!);
            if (oldPageFirstNumber is null || oldPageFirstNumber <= 1)
            {
                return;
            }

            int startWith;
            if (oldPageFirstNumber <= 10)
            {
                startWith = 1;
            }
            else
            {
                startWith = oldPageFirstNumber.Value - 10;
            }

            string page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static async Task TurnDeletePageForwardAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            IEnumerable<ChatInfo> chatInfos = chats.Select(c =>
                new ChatInfo { Id = c.TelegramChatId, Title = c.Title });

            int? oldPageLastNumber = ParseLastNumber(callbackQuery.Message!.Text!);
            if (oldPageLastNumber is null || oldPageLastNumber >= chatInfos.Count())
            {
                return;
            }
            int startWith = oldPageLastNumber.Value + 1;

            var page = GetChatPage(chatInfos, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static async Task HandleChatDeletionSelectionAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string[] chatIds = message.Text!.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (chatIds.Length == 0)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "No chat IDs provided. Please send at least one ID.",
                    botClient, logger, cancellationToken);
                return;
            }
            List<long> removedChats = [];
            foreach (string chatId in chatIds)
            {
                if (long.TryParse(chatId, out long parsedChatId))
                {
                    removedChats.Add(parsedChatId);
                }
                else
                {
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        $"Invalid chat ID: {chatId}. Please provide valid numeric IDs.",
                        botClient, logger, cancellationToken);
                }
            }
            if (removedChats.Count == 0)
            {
                return;
            }
            FieldUpdateResult result = await userbotApiService.RemoveChatsAsync(user.TelegramUserId, removedChats);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to untrack chats: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }
            await userService.RemoveUserChatsAsync(user.TelegramUserId, removedChats);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "Chats successfully untracked!",
                botClient, logger, cancellationToken);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            user = await userService.GetUserAsync(user.TelegramUserId);
            await RunChatMenuAsync(false, user, message, userService, userbotApiService,
                botClient, logger, cancellationToken);
        }
    }
}
