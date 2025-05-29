using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public static class BotHelper
    {
        public static async Task SendTextMessageAsync(
            long chatId, string text,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken,

            ReplyMarkup? replyMarkup = null,
            ParseMode parseMode = ParseMode.MarkdownV2)
        {
            try
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: parseMode == ParseMode.MarkdownV2 ? EscapeMarkdownV2(text) : text,
                    replyMarkup: replyMarkup,
                    parseMode: parseMode,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            }
        }

        public static string EscapeMarkdownV2(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var specialChars = new[] { '\\', '_', /*'*',*/ '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

            var builder = new System.Text.StringBuilder();

            foreach (var ch in text)
            {
                if (Array.IndexOf(specialChars, ch) != -1)
                    builder.Append('\\');
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string GetUserNameOrEmpty(BotUser user)
        {
            string prefix = user.UserName ?? user.UserName ?? string.Empty;
            prefix = prefix != string.Empty ? $"{prefix}," : string.Empty;
            return prefix;
        }
    }
}
