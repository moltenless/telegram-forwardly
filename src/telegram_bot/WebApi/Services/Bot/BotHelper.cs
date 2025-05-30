using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;

namespace TelegramForwardly.WebApi.Services.Bot
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

        public static string GetUserNameOrEmpty(BotUser user, string ending)
        {
            string prefix = user.FirstName ?? user.UserName ?? string.Empty;
            prefix = prefix != string.Empty ? $"{prefix},{ending}" : string.Empty;
            return prefix;
        }


        public static string GetWelcomeMessage(BotUser user)
            => $"{GetUserNameOrEmpty(user, " ")}Welcome to Telegram Forwardly! 🚀\n\n" +
                                "I'll help you forward important messages from active Telegram groups or channels to your organized forum.\n\n" +
                                "*To get started*, you need to set up *your account information* and Telegram API credentials.\n\n" +
                                "To begin the configuration process:\n*Please click \"🔑 Setup credentials\" in menu below*\nOr\n*Use /setup*";

        public static string GetSetupMessage()
            => "Let's get started with the phone number linked to your Telegram account:\n\n" +
                             "*Please click \"Share phone number 📞🔢\" button below*\nOr\n*Enter manually in the format:* +1234567890 or 1234567890";

        public static string GetApiIdMessage()
            => "Now, let's set up your Telegram API ID and API Hash:\n\n" +
                         "First, I need your API ID. You can get it from official Telegram website https://my.telegram.org/apps\n\n" +
                         "Please send me your API ID:";

        public static InlineKeyboardMarkup GetMenuKeyboard()
            => new([
            [
                InlineKeyboardButton.WithCallbackData("🔑 Setup credentials", "setup"),
                InlineKeyboardButton.WithCallbackData("📝 Keywords", "keywords")
            ],
            [
                InlineKeyboardButton.WithCallbackData("💬 Chats", "chats"),
                InlineKeyboardButton.WithCallbackData("📊 Status", "status"),
            ],
            [
                InlineKeyboardButton.WithCallbackData("⚙️ Settings", "settings"),
                InlineKeyboardButton.WithCallbackData("❓ Help", "help")
            ]
            ]);

        public static ReplyKeyboardMarkup GetPhoneKeyboard()
            => new(new KeyboardButton("Share phone number 📞🔢")
            { RequestContact = true })
            { ResizeKeyboard = true, OneTimeKeyboard = true, };
    }
}
