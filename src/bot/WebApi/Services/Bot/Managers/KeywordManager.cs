using System.Net.WebSockets;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class KeywordManager
    {
        public static async Task EnterKeywordsAsync(
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
                [InlineKeyboardButton.WithCallbackData("⬅️ Page Back", "view_keywords_page_back"),
                    InlineKeyboardButton.WithCallbackData("Page Forward ➡️", "view_keywords_page_forward")],
                [ InlineKeyboardButton.WithCallbackData("➕ Add Keyword", "add_keyword"),
                    InlineKeyboardButton.WithCallbackData("🗑️ Remove Keyword", "remove_keyword") ],
                [ InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu") ]
            ]);

            await botClient.DeleteMessage(
                    chatId: message.Chat.Id, message.MessageId, cancellationToken);
            await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, "Your keywords:",
                    botClient, logger, cancellationToken);
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, startPage, botClient,
                logger, cancellationToken, replyMarkup: keyboard);
        }

        ////// IF -> either keyword.value or bothelper.removespecialcharacters(keyword.value)

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

                sb.Append($"> {i} ~ `{BotHelper.RemoveSpecialChars(keyword.Value)}`");
            }

            return sb.ToString();
        }
    }
}
