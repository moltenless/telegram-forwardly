using Telegram.Bot.Types;
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
                        sendTextMessageAsync, userService, cancellationToken);
                    break;
                case "/keywords":

                    break;
                case "/chats":

                    break;
                case "/status":

                    break;
                case "/help":

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
                                $"To get started, you need to set up your Telegram API credentials.\n" +
                                $"Use /setup to begin the configuration process.";

            await sendTextMessageAsync(message.Chat.Id, welcomeMessage, cancellationToken);
            await showMainMenuAsync(user, message.Chat.Id, cancellationToken);
        }

        private static async Task HandleSetupCommandAsync(BotUser user, Message message,
            Func<long, string, CancellationToken, Task> sendTextMessageAsync,
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

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingApiId);

            var setupMessage = "Let's set up your Telegram API ID and API Hash to continue:\n\n" +
                             "First, I need your API ID. You can get it from official Telegram website https://my.telegram.org/apps\n\n" +
                             "Please send me your API ID:";

            await sendTextMessageAsync(message.Chat.Id, setupMessage, cancellationToken);
        }
    }
}
