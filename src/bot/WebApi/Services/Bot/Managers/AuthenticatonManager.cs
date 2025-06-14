using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class AuthenticationManager
    {
        public static async Task HandlePhoneInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string? phone = message.Contact is not null
                ? message.Contact.PhoneNumber
                : message.Text?.Trim();
            await userService.UpdateUserPhoneAsync(user.TelegramUserId, phone!);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingApiId);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                BotHelper.GetApiIdMessage(user.ApiId),
                botClient, logger,
                cancellationToken,
                new ReplyKeyboardRemove());
        }

        public static async Task HandleApiIdInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string apiId = message.Text?.Trim() ?? string.Empty;
            await userService.UpdateUserApiIdAsync(user.TelegramUserId, apiId);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingApiHash);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                BotHelper.GetApiHashMessage(user.ApiHash),
                botClient, logger,
                cancellationToken);
        }

        public static async Task HandleApiHashInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string apiHash = message.Text?.Trim() ?? string.Empty;
            await userService.UpdateUserApiHashAsync(user.TelegramUserId, apiHash);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingSessionString);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                BotHelper.GetSessionStringMessage(user.SessionString),
                botClient, logger,
                cancellationToken);
        }

        public static async Task HandleSessionStringInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string sessionString = message.Text!.Trim();
            await userService.UpdateUserSessionStringAsync(user.TelegramUserId, sessionString);

            await CompleteAuthenticationAsync(
                user.TelegramUserId, message.Chat.Id,
                userService, userbotApiService, botClient, logger, cancellationToken);
        }


        private static async Task CompleteAuthenticationAsync(
            long telegramUserId,
            long chatId,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            BotUser user = await userService.GetUserAsync(telegramUserId);

            LaunchResult result =
                await userbotApiService.LaunchUserAsync(user);

            if (!result.Success)
            {
                await userService.SetUserAuthenticatedAsync(telegramUserId, false);
                await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
                user = await userService.GetUserAsync(telegramUserId);

                string errorMessage = user.SessionString!.Trim().EndsWith('=') ? string.Empty : "Usually session strings end with '='\n";
                await BotHelper.SendTextMessageAsync(
                    chatId, errorMessage + $"❌ Authentication failed: {result.ErrorMessage}. Make sure that new active session appear in Telegram Settings -> Privacy and Security.",
                    botClient, logger, cancellationToken);
                await MenuManager.ShowMainMenuAsync(user, chatId, botClient, logger, cancellationToken);
                return;
            } 

            await userService.SetUserAuthenticatedAsync(telegramUserId, true);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
            user = await userService.GetUserAsync(telegramUserId);

            await BotHelper.SendTextMessageAsync(
                chatId,
                user.ForumSupergroupId == null
                ? BotHelper.GetAuthenticatedAndSettingsMessage()
                : BotHelper.GetAuthenticatedMessage(),
                botClient, logger, cancellationToken);
            await MenuManager.ShowMainMenuAsync(user, chatId, botClient, logger, cancellationToken);
        }


        public static async Task HandleDeleteConfirmationInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var conf = message.Text?.Trim().ToLowerInvariant();

            if (conf == "yes, I want this bot not to persist my data".ToLowerInvariant())
            {
                FieldUpdateResult result = await userbotApiService.DeleteUserAsync(user.TelegramUserId);
                if (!result.Success)
                {
                    await BotHelper.SendTextMessageAsync(
                        message.Chat.Id,
                        $"Failed to delete user data: {result.ErrorMessage}",
                        botClient, logger, cancellationToken, parseMode: ParseMode.None);
                    return;
                }
                await userService.DeleteUserAsync(user.TelegramUserId);
                user = await userService.GetOrCreateUserAsync(message.From!.Id, UserState.Idle, message.From!.Username, message.From!.FirstName);
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Your data has been successfully deleted. You can set up the bot again with /setup.",
                    botClient, logger, cancellationToken);
                await MenuManager.ShowMainMenuAsync(user, message.Chat.Id, botClient, logger, cancellationToken);
            }
            else
            {
                await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Your data has not been deleted. You can continue using the bot.",
                    botClient, logger, cancellationToken);
                await MenuManager.ShowMainMenuAsync(user, message.Chat.Id, botClient, logger, cancellationToken);
            }
        }





        public static async Task<string> RequestAndAwaitVerificationCodeAsync(
            long telegramUserId,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(
                chatId, "The verification code has been sent. Please write it down",
                botClient, logger, cancellationToken);

            await userService.SetUserStateAsync(telegramUserId, UserState.Idle);

            int interval = 3, maxAttempts = 60;
            for (int i = 0; i < maxAttempts; i++)
            {
                await Task.Delay(interval * 1000, cancellationToken);

                string? code = "";
                if (code is not null)
                {
                    return code;
                }
            }
            throw new TimeoutException(
                "Verification code input timed out. Please try again with /setup.");
        }
    }
}
