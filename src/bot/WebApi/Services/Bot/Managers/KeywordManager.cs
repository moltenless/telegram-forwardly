using System.Net.WebSockets;
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
    public static class KeywordManager
    {
        public static async Task EnterKeywordsAsync(
            bool editSourceMessage,
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);
            var startPage = GetKeywordPage(keywords, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "keyword_view_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "keyword_view_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("➕ Add Keyword", "add_keyword"),
                    InlineKeyboardButton.WithCallbackData("🗑️ Remove Keyword", "remove_keyword") ],
                [ InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu") ]
            ]);

            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    chatId: message.Chat.Id, messageId: message.MessageId,
                    text: BotHelper.DefaultEscapeMarkdownV2(startPage),
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

        public static async Task TurnViewPageBackAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);

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

            var page = GetKeywordPage(keywords, startWith, itemsCount: 10);

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
            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);

            int? oldPageLastNumber = ParseLastNumber(callbackQuery.Message!.Text!);
            if (oldPageLastNumber is null || oldPageLastNumber >= keywords.Count)
            {
                return;
            }

            int startWith = oldPageLastNumber.Value + 1;
            var page = GetKeywordPage(keywords, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }

        public static string GetKeywordPage(
            IEnumerable<Keyword> keywords,
            int startWith,
            int itemsCount)
        {
            if (!keywords.Any()) return "No keywords added yet.";

            StringBuilder sb = new();
            for (int i = startWith; i < startWith + itemsCount; i++)
            {
                if (i - 1 >= keywords.Count()) break;

                var keyword = keywords.ElementAt(i - 1);
                if (keyword is null) continue;

                sb.Append($"> {i} ~ `{BotHelper.RemoveSpecialChars(keyword.Value)} `\n");
            }

            return sb.ToString();
        }

        private static int? ParseFirstNumber(string page)
        {
            try
            {
                string[] lines = page.Split(">",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string[] splitFirstLine = lines[0].Split(["~"],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                return int.Parse(splitFirstLine[0].Trim());
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
                string[] splitLastLine = lines[^1].Split(["~"],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return int.Parse(splitLastLine[0].Trim());
            }
            catch
            {
                return null;
            }
        }

        ////// IF -> either keyword.value or bothelper.removespecialcharacters(keyword.value)

        public static async Task StartKeywordAdditionAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(
            [[InlineKeyboardButton.WithCallbackData("📝 Back to Keyword Menu", "back_to_keyword_menu")]]);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(
                    "*Send* me keywords you want to add. Separate them with *commas*." +
                      "\n\nFor example: *`keyword1, hello world`*"),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingKeywords);
        }

        public static async Task HandleKeywordAdditionAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string[] input = message.Text!.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<string> keywords = [];
            foreach (string keyword in input)
            {
                if (string.IsNullOrEmpty(keyword) || keyword.Trim().Length < 2)
                {
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        $"Invalid keyword: {keyword}. Please provide valid keyword (it must contain more than 1 character).",
                        botClient, logger, cancellationToken, parseMode: ParseMode.None);
                }
                else
                    keywords.Add(keyword.Trim());
            }
            if (keywords.Count == 0)
            {
                await BotHelper.SendTextMessageAsync(
                     message.Chat.Id,
                     "No keywords provided. Please send at least one keyword.",
                     botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            FieldUpdateResult result = await userbotApiService.AddKeywordsAsync(
                user.TelegramUserId, [.. keywords]);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to add keywords: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }
            await userService.AddUserKeywordsAsync(user.TelegramUserId, [.. keywords]);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
            user = await userService.GetUserAsync(user.TelegramUserId);

            ///////////// FURTHER SETUP PROCESS IF NEEDED /////////////
            if (user.Keywords.Count == 0)
            {
                //await BotHelper.SendTextMessageAsync(
                //    message.Chat.Id,
                //    "Chats successfully added!\n\n" +
                //    "Now, please add some keywords to filter messages in chats.\n" +
                //    "*Please click '📝 Keywords' in menu below*\nOr\n*Use /keywords*",
                //    botClient, logger, cancellationToken);
                //await MenuManager.ShowMainMenuAsync(
                //    user, message.Chat.Id,
                //    botClient, logger, cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Keywords successfully added!",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                await EnterKeywordsAsync(false, user, message, userService,
                    botClient, logger, cancellationToken);
            }
        }

        public static async Task StartKeywordDeletionAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);
            var startPage = GetKeywordPage(keywords, startWith: 1, itemsCount: 10);

            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "keyword_deletion_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "keyword_deletion_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("📝 Back to Keyword Menu", "back_to_keyword_menu")]]);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(startPage),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            await BotHelper.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                "*Send* me keywords you want to REMOVE ⬆️. \nTap to copy it. Separate them with *commas*." +
                "\n\nFor example: *`keyword1, hello world`*\"",
                botClient, logger, cancellationToken);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingRemoveKeywords);
        }

        public static async Task HandleKeywordDeletionAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string[] input = message.Text!.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<string> keywordsWithoutSpecialCharacters = [];
            foreach (string keyword in input)
            {
                if (string.IsNullOrEmpty(keyword) || keyword.Trim().Length < 2)
                {
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        $"Invalid keyword: {keyword}. Please provide valid keyword (it must contain more than 1 character).",
                        botClient, logger, cancellationToken, parseMode: ParseMode.None);
                }
                else
                    keywordsWithoutSpecialCharacters.Add(keyword.Trim());
            }
            if (keywordsWithoutSpecialCharacters.Count == 0)
            {
                await BotHelper.SendTextMessageAsync(
                     message.Chat.Id,
                     "No keywords provided. Please send at least one keyword.",
                     botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            FieldUpdateResult result = await userbotApiService.RemoveKeywordsAsync(
                user.TelegramUserId, [.. keywordsWithoutSpecialCharacters]);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to remove keywords: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            await userService.RemoveKeywordsAsync(
                user.TelegramUserId, 
                [.. keywordsWithoutSpecialCharacters],
                BotHelper.RemoveSpecialChars);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "Keywords successfully removed!",
                botClient, logger, cancellationToken, parseMode: ParseMode.None);

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
            user = await userService.GetUserAsync(user.TelegramUserId);
            await EnterKeywordsAsync(false, user, message, userService, 
                botClient, logger, cancellationToken);
        }

        public static async Task TurnDeletePageBackAsync(BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);

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

            string page = GetKeywordPage(keywords, startWith, itemsCount: 10);

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
            var keywords = await userService.GetUserKeywordsAsync(user.TelegramUserId);

            int? oldPageLastNumber = ParseLastNumber(callbackQuery.Message!.Text!);
            if (oldPageLastNumber is null || oldPageLastNumber >= keywords.Count)
            {
                return;
            }
            int startWith = oldPageLastNumber.Value + 1;

            var page = GetKeywordPage(keywords, startWith, itemsCount: 10);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: BotHelper.DefaultEscapeMarkdownV2(page),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: cancellationToken);
        }
    }
}
