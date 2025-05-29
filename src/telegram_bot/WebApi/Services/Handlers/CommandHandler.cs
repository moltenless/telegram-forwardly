using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;
using TelegramForwardly.WebApi.Services.Interfaces.Handlers;

namespace TelegramForwardly.WebApi.Services.Handlers
{
    public class CommandHandler : ICommandHandler
    {
        public async Task HandleCommandAsync(BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            Func<BotUser, long, CancellationToken, Task> showMainMenuAsync,
            ITelegramBotClient botClient,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            var command = message.Text!.Split(' ')[0].ToLower();

            switch (command)
            {
                case "/start":
                    await HandleStartCommandAsync(user, message,
                        sendTextMessageAsync, showMainMenuAsync, cancellationToken);
                    break;
                case "/menu":
                    await showMainMenuAsync(user, message.Chat.Id, cancellationToken);
                    break;
                case "/setup":
                    await HandleSetupCommandAsync(user, message,
                        sendTextMessageAsync, botClient,
                        userService, cancellationToken);
                    break;
                case "/keywords":
                    await HandleKeywordsCommandAsync(user, message,
                        sendTextMessageAsync,
                        cancellationToken);
                    break;
                case "/chats":
                    await HandleChatsCommandAsync(user, message,
                        sendTextMessageAsync,
                        cancellationToken);
                    break;
                case "/status":
                    await HandleStatusCommandAsync(user, message,
                        sendTextMessageAsync,
                        cancellationToken);
                    break;
                case "/settings":
                    await HandleSettingsCommandAsync(user, message,
                        sendTextMessageAsync,
                        cancellationToken);
                    break;
                case "/help":
                    await HandleHelpCommandAsync(user, message,
                        sendTextMessageAsync,
                        cancellationToken);
                    break;
                default:
                    await sendTextMessageAsync(message.Chat.Id,
                        "Unknown command. Use /help to see available commands.",
                        cancellationToken);
                    break;
            }
        }

        private static async Task HandleStartCommandAsync(BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            Func<BotUser, long, CancellationToken, Task> showMainMenuAsync,
            CancellationToken cancellationToken)
        {
            var welcomeMessage = $"Welcome to Telegram Forwardly! 🚀\n\n" +
                                $"I'll help you forward important messages from active Telegram groups or channels to your organized forum.\n\n" +
                                $"*To get started*, you need to set up *your account information* and Telegram API credentials.\n\n" +
                                $"Please click *🔑 Setup credentials* button below\n*Or*\nUse /setup to begin the configuration process.";

            await sendTextMessageAsync(message.Chat.Id, welcomeMessage, cancellationToken);
            await showMainMenuAsync(user, message.Chat.Id, cancellationToken);
        }

        private static async Task HandleSetupCommandAsync(BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            ITelegramBotClient botClient,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            if ((bool)user.IsAuthenticated!)
            {
                await sendTextMessageAsync(message.Chat.Id,
                    "You're already authenticated! Use /menu to see available options.",
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

            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: BotService.EscapeMarkdownV2(setupMessage),
                replyMarkup: keyboard,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }

        private static async Task HandleKeywordsCommandAsync(
            BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            CancellationToken cancellationToken)
        {
            await sendTextMessageAsync(message.Chat.Id,
                "You want to manage your keywords? This feature is not implemented yet. ",
                cancellationToken);
        }

        private static async Task HandleChatsCommandAsync(
            BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            CancellationToken cancellationToken)
        {
            await sendTextMessageAsync(message.Chat.Id,
                "You want to manage your chats? This feature is not implemented yet. ",
                cancellationToken);
        }

        private static async Task HandleStatusCommandAsync(
            BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            CancellationToken cancellationToken)
        {
            await sendTextMessageAsync(message.Chat.Id,
                "Check your status? This feature is not implemented yet. ",
                cancellationToken);
        }

        private async Task HandleSettingsCommandAsync(BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            CancellationToken cancellationToken)
        {
            await sendTextMessageAsync(message.Chat.Id,
                "Settings will be here. This feature is not implemented yet. ",
                cancellationToken);
        }

        private static async Task HandleHelpCommandAsync(
            BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
            CancellationToken cancellationToken)
        {
            await sendTextMessageAsync(message.Chat.Id,
                "Soon here will be help topic. This feature is not implemented yet. ",
                cancellationToken);
        }
    }
}
