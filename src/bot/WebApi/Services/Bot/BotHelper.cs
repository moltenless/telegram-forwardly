using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        ParseMode parseMode = ParseMode.MarkdownV2,
        Func<string, string>? markdownV2escaper = null)
        {
            try
            {
                markdownV2escaper ??= DefaultEscapeMarkdownV2;

                await botClient.SendMessage(
                    chatId: chatId,
                    text: parseMode == ParseMode.MarkdownV2 ? markdownV2escaper(text) : text,
                    replyMarkup: replyMarkup,
                    parseMode: parseMode,
                    linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send message to chat {ChatId}: {Message}", chatId, text);
            }
        }

        public static string DefaultEscapeMarkdownV2(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var specialChars = new[] { '\\', /*'_', '*', '[', ']', '(', ')',*/ '~', /*'`',*/ '>', '#', '+', '-', '=', /*'|',*/ '{', '}', '.', '!' };

            var builder = new StringBuilder();

            foreach (var ch in text)
            {
                if (Array.IndexOf(specialChars, ch) != -1)
                    builder.Append('\\');
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string RemoveSpecialChars(string input)
        {
            var specialChars = new[] { '\\', '_', '*', '[', ']', '(', ')', '~', '`', '>', '<', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            //var specialChars = new[] { '_', '*', '[', ']', '(', ')', '`', '|' };
            var sb = new StringBuilder();
            foreach (char c in input)
                if (!specialChars.Contains(c))
                    sb.Append(c);
            return sb.ToString();
        }

        public static string GetUserNameOrEmpty(BotUser user, string ending)
        {
            string prefix = user.FirstName ?? user.UserName ?? string.Empty;
            prefix = prefix != string.Empty ? $"{prefix},{ending}" : string.Empty;
            return prefix;
        }

        public static string GetMenuText(BotUser user)
            => (bool)user.IsAuthenticated!
                ? "🏠 Main Menu - You're authenticated and ready to go!"
                : "🏠 Main Menu - Please set up your credentials first.";

        public static string GetWelcomeMessage(BotUser user)
            => $"{GetUserNameOrEmpty(user, " ")}" +
            $"Welcome to Telegram Forwardly! 🚀\n\n" +
              "I'll help you forward important messages from active Telegram groups or channels to your organized forum.\n\n" +
              "*To get started*, you need to set up *your account information* and Telegram API credentials.\n\n" +
              "To begin the configuration process:\n*Please click \'🔑 Setup credentials\' in menu below*\nOr\n*Use /setup*";

        public static string GetSetupMessage()
            => "Let's get started with the phone number linked to your Telegram account:\n\n" +
                             "*Please click \'Share phone number 📞🔢\' button below*\nOr\n*Enter manually in the format:* +1234567890 or 1234567890 with country code";

        public static string GetApiIdMessage(string phone)
            => $"*Now, let's set up your Telegram API ID and API Hash:*\n\n" +
                "You can get them from official Telegram website https://my.telegram.org\n" +
                "1. There you will need to log in with your phone number and confirm with \'login code\' sent by Telegram.\n" +
                "2. Then click _API development tools_.\n3. Under _App title_ and _Short name_, " +
                "name them whatever you want, for example 'app' or 'forwardly'. " +
                "For the _Platform_, select \'Desktop\' or \'Web\'. Remain other fields empty.\n" +
                "4. Click _Create application_ and then copy your *App api-id*.\n\n" +
                "_! Please note that you should not share your API Id and API Hash with anyone! " +
                "We don't use them for anything other than forwarding messages to your forum. " +
                "Check out our open source code on [GitHub](https://github.com/moltenless/telegram-forwardly). " +
                "You can always delete them in bot settings menu or revoke access to your app in Telegram settings._\n\n" +
                "*First, please send me the App api-id:* it looks like '12345678'";

        public static string GetApiHashMessage()
            => $"Perfect! Now, please send me your *App api-hash*.\n\n" +
                "You can copy it in the same place where you got your App api-id. " +
                "it looks like `0123456789abcdef0123456789abcdef`";

        public static string GetSessionStringMessage()
            => $"Awesome! Now, please send me your *Session String*.\n\n" +
                "The safest way to get it is by using a transparent script in this " +
                "[generator](https://colab.research.google.com/drive/1lxctt9F8QN0jbAcod12ljCo0QoW6MnYN?usp=sharing). " +
                "It’s most convenient to use from a laptop.\n" +
                "👉 Open the link, sign in with your Google account " +
                "and press ▶️‍ button at the top. Under the code you'll be asked to enter your API ID etc.\n" +
                "After completing you will get _session string_. *Copy and paste it here.*\n\n" +

                "_Alternatively, there's a third-party service that can generate session string more easily: " +
                "[service](https://telegram.tools/session-string-generator#telethon) - choose 'User' as 'Account Type'.\n" +
                "*However, it’s not officially verified and may pose a security risk. Use it at your own risk and discretion.*_\n\n";

        public static string GetPasswordMessage()
            => $"Great! If you have enabled 2FA protection for this account please enter password:\n\n" +
                "If you don't have one - simply skip this part and respond with *'no' or 'No'*\n\n";

        public static string GetAuthenticatedMessage()
            => "🎉 Authentication successful!";

        public static string GetAuthenticatedAndSettingsMessage()
            => "🎉 Authentication successful!\n\n" +
               "*Now let's set up forwarding details*:\n" +
               "*Please click '⚙️ Settings' in menu below*\nOr\n*Use /settings*";

        public static string GetSettingsMessage()
            => "*Now, create your own group in Telegram* - so I can set this group as your forum supergroup with topics where messages will be forwarded.\n\n" +
               "1. Create a new group in Telegram. Call it something like 'Forwardly'\n" +
               "2. Go to group settings and click on 'Topics' section.\n" +
               "3. Enable this option and choose style of topic layout you prefer. Select 'List' for now. Save the changes.\n" +
               "4. Then, here in this chat with me click on 'Add to Group'. Select your recently created group. It will immediately send there a message with the group ID.\n" +
               "5. Please, copy the group id - *it looks like '-100XXXXXXXXX' - and send it to me here*";

        public static InlineKeyboardMarkup GetMenuKeyboard(bool forwardlyEnabled)
            => new([
            [
                InlineKeyboardButton.WithCallbackData("🔑 Setup credentials", "setup"),
                InlineKeyboardButton.WithCallbackData("⚙️ Settings", "settings"),
            ],
            [
                InlineKeyboardButton.WithCallbackData("💬 Chats", "chats"),
                InlineKeyboardButton.WithCallbackData("📝 Keywords", "keywords")
            ],
            [
                InlineKeyboardButton.WithCallbackData("📊 Status", "status"),
                InlineKeyboardButton.WithCallbackData(forwardlyEnabled ? "⏸️ Pause forwarding" : "▶️ Resume forwarding", "forwardly_enabled"),
                InlineKeyboardButton.WithCallbackData("❓ Help", "help")
            ]
            ]);

        public static ReplyKeyboardMarkup GetPhoneKeyboard()
            => new(new KeyboardButton("Share phone number 📞🔢")
            { RequestContact = true })
            { ResizeKeyboard = true, OneTimeKeyboard = true, };

        /*
            'The verification code has been sent to you by Telegram official chat. '
            'It also could be sent on your phone SMS.'
            '\nEnter that code: '

            'It seems like your account has enabled 2FA protection measures.'
            '\nPlease provide with password to your account to get complete authentication: '
        */
    }
}
