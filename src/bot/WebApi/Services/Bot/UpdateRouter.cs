using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot
{
    public static class UpdateRouter
    {
        public static async Task RouteCommandAsync(BotUser user,
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
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id, BotHelper.GetWelcomeMessage(user),
                        botClient, logger, cancellationToken);
                    await MenuManager.ShowMainMenuAsync(
                        user, message.Chat.Id,
                        botClient, logger, cancellationToken);
                    break;

                case "/menu":
                    await MenuManager.ShowMainMenuAsync(
                        user, message.Chat.Id,
                        botClient, logger, cancellationToken);
                    break;

                case "/setup":
                    await MenuManager.EnterSetupAsync(
                        user, message.Chat.Id,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "/keywords":
                    break;

                case "/chats":
                    break;

                case "/status":
                    break;

                case "/settings":
                    break;

                case "/help":
                    break;

                default:
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        "Unknown command. Use /help to see available commands.",
                        botClient, logger, cancellationToken);
                    break;
            }
        }

        public static async Task RouteCallbackQueryAsync(
            BotUser user,
            CallbackQuery callbackQuery,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            switch (callbackQuery.Data!)
            {
                case "setup":
                    await MenuManager.EnterSetupAsync(
                        user, callbackQuery.Message!.Chat.Id,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "keywords":
                    break;

                case "chats":
                    break;

                case "status":
                    break;

                case "settings":
                    break;

                case "help":
                    break;
            }
        }

        public static async Task RouteUserInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IAuthApiService authApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            switch (user.CurrentState)
            {
                case UserState.AwaitingPhoneNumber:
                    await AuthenticationManager.HandlePhoneInputAsync(
                        user, message, userService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingApiId:
                    await AuthenticationManager.HandleApiIdInputAsync(
                        user, message, userService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingApiHash:
                    await AuthenticationManager.HandleApiHashInputAsync(
                        user, message, userService, authApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingPassword:
                    await AuthenticationManager.HandlePasswordInputAsync(
                        user, message, userService, authApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingVerificationCode:
                    await AuthenticationManager.HandleVerificationCodeInputAsync(
                        user, message, userService, authApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingEnableAllChats:
                    break;

                case UserState.AwaitingChats:
                    break;

                case UserState.AwaitingKeywords:
                    break;

                case UserState.AwaitingForumGroup:
                    break;

                case UserState.AwaitingGroupingType:
                    break;

                case UserState.AwaitingEnableLoggingTopic:
                    break;

                default:
                    await MenuManager.ShowMainMenuAsync(
                        user, message.Chat.Id,
                        botClient, logger, cancellationToken);
                    break;
            }
        }
    }
}
