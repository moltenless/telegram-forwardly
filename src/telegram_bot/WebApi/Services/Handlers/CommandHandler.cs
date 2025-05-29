using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public static class CommandHandler
    {
        public static async Task HandleCommandAsync(
            BotUser user, 
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var command = message.Text!.Split(' ')[0].ToLower();

            switch (command)
            {
                case "/start":
                    await HandleStartCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/menu":
                    await ShowMainMenuAsync(
                        user, message.Chat.Id,
                        botClient, logger, 
                        cancellationToken);
                    break;
                case "/setup":
                    await HandleSetupCommandAsync(
                        user, message,
                        userService,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/keywords":
                    await HandleKeywordsCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/chats":
                    await HandleChatsCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/status":
                    await HandleStatusCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/settings":
                    await HandleSettingsCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                case "/help":
                    await HandleHelpCommandAsync(
                        user, message,
                        botClient, logger,
                        cancellationToken);
                    break;
                default:
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        "Unknown command. Use /help to see available commands.",
                        botClient, logger,
                        cancellationToken);
                    break;
            }
        }

        private static async Task HandleStartCommandAsync(
            BotUser user,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var welcomeMessage = $"{BotHelper.GetUserNameOrEmpty(user)} Welcome to Telegram Forwardly! 🚀\n\n" +
                                $"I'll help you forward important messages from active Telegram groups or channels to your organized forum.\n\n" +
                                $"*To get started*, you need to set up *your account information* and Telegram API credentials.\n\n" +
                                $"Please click *🔑 Setup credentials* button below\n*Or*\nUse /setup to begin the configuration process.";

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, welcomeMessage,
                botClient, logger,
                cancellationToken);
            await ShowMainMenuAsync(
                user, message.Chat.Id,
                botClient, logger,
                cancellationToken);
        }

        public static async Task ShowMainMenuAsync(
            BotUser user, 
            long chatId,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup([
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

            var menuText = (bool)user.IsAuthenticated!
                ? "🏠 Main Menu - You're authenticated and ready to go!"
                : "🏠 Main Menu - Please set up your credentials first.";

            await BotHelper.SendTextMessageAsync(
                chatId, menuText,
                botClient, logger,
                cancellationToken,
                keyboard);
        }

        private static async Task HandleSetupCommandAsync(
            BotUser user, 
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if ((bool)user.IsAuthenticated!)
            {
                await BotHelper.SendTextMessageAsync(message.Chat.Id, 
                    "You're already authenticated! Use /menu to see available options.",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingPhoneNumber);

            var setupMessage = "Let's get started with the phone number linked to your Telegram account:\n\n" +
                             "Please click *Share phone number 📞🔢* button below\n*Or*\nEnter manually in the format: +1234567890 or 1234567890";

            //"Now, let's set up your Telegram API ID and API Hash:\n\n" +
            //                 "First, I need your API ID. You can get it from official Telegram website https://my.telegram.org/apps\n\n" +
            //                 "Please send me your API ID:"

            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton("Share phone number 📞🔢")
            { RequestContact = true })
            { ResizeKeyboard = true, OneTimeKeyboard = true, };

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                setupMessage,
                botClient, logger,
                cancellationToken,
                keyboard);
        }

        private static async Task HandleKeywordsCommandAsync(
            BotUser user, 
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(message.Chat.Id,
                $"{BotHelper.GetUserNameOrEmpty(user)}You want to manage your keywords? This feature is not implemented yet. ",
                botClient, logger,
                cancellationToken);
        }

        private static async Task HandleChatsCommandAsync(
            BotUser user,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(message.Chat.Id,
                $"{BotHelper.GetUserNameOrEmpty(user)}You want to manage your chats? This feature is not implemented yet. ",
                botClient, logger,
                cancellationToken);
        }

        private static async Task HandleStatusCommandAsync(
            BotUser user,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(message.Chat.Id,
                $"{BotHelper.GetUserNameOrEmpty(user)}Check your status? This feature is not implemented yet.",
                botClient, logger,
                cancellationToken);
        }

        private static async Task HandleSettingsCommandAsync(
            BotUser user,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(message.Chat.Id,
                $"{BotHelper.GetUserNameOrEmpty(user)}Settings will be here. This feature is not implemented yet.",
                botClient, logger,
                cancellationToken);
        }

        private static async Task HandleHelpCommandAsync(
            BotUser user,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(message.Chat.Id,
                $"{BotHelper.GetUserNameOrEmpty(user)} Soon here will be help topic. This feature is not implemented yet.",
                botClient, logger,
                cancellationToken);
        }
    }
}
