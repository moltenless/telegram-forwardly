using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot
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

            if (string.IsNullOrEmpty(phone))
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Invalid phone number format.",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.UpdateUserPhoneAsync(user.TelegramUserId, phone);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingApiId);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                BotHelper.GetApiIdMessage(phone),
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
                BotHelper.GetApiHashMessage(),
                botClient, logger,
                cancellationToken);
        }

        public static async Task HandleApiHashInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IAuthApiService authApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string apiHash = message.Text?.Trim() ?? string.Empty;
            await userService.UpdateUserApiHashAsync(user.TelegramUserId, apiHash);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingPassword);
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                BotHelper.GetPasswordMessage(),
                botClient, logger,
                cancellationToken);
        }

        public static async Task HandlePasswordInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IAuthApiService authApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string? password = message.Text!.Trim();
            if (password == "no" || password == "No")
                password = null;
            await userService.UpdateUserPasswordAsync(user.TelegramUserId, password);

            string apiId = user.ApiId!;
            string phone = user.Phone!;
            string apiHash = user.ApiHash!;

            await userService.RemoveUserVerificationCode(user.TelegramUserId);/////////////
            authApiService.StartAuthenticationAsync(
                user.TelegramUserId, phone, apiId, apiHash, password);
                
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingVerificationCode);
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "A verification code has been sent to your Telegram account. *Please send me the code:*",
                botClient, logger,
                cancellationToken);
        }

        public static async Task HandleVerificationCodeInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IAuthApiService authApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var verificationCode = message.Text!.Trim();

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "Verifying your account. Please wait..",
                botClient, logger,
                cancellationToken);

            await userService.UpdateUserVerificationCodeAsync(
                user.TelegramUserId, verificationCode);


            //var authResult = await authApiService.VerifyCodeAsync(
            //    user.TelegramUserId, verificationCode);

            //if (!authResult.Success)
            //{
            //    if (authResult.RequiresPassword)
            //    {
            //        await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingPassword);
            //        await BotHelper.SendTextMessageAsync(message.Chat.Id,
            //            "Two-factor authentication is enabled. Please send me your *password* linked to this account:",
            //            botClient, logger,
            //            cancellationToken);
            //        return;
            //    }

            //    await BotHelper.SendTextMessageAsync(message.Chat.Id,
            //        $"Verification failed: {authResult.ErrorMessage}\nPlease try again with /setup",
            //        botClient, logger,
            //        cancellationToken, parseMode: ParseMode.None);
            //    await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
            //    return;
            //}

            //await CompleteAuthenticationAsync(
            //    user, authResult.SessionString!,
            //    message.Chat.Id, userService, 
            //    botClient, logger, cancellationToken);
        }

        private static async Task CompleteAuthenticationAsync(
            BotUser user,
            string sessionString,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.CompleteAuthenticationAsync(user.TelegramUserId, sessionString);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            var successMessage = "🎉 Authentication successful!\n\n" +
                               "You can now:\n" +
                               "• Manage keywords with /keywords\n" +
                               "• Select chats to monitor with /chats\n" +
                               "• Check your status with /status\n" +
                               "• Use /menu to see all options";

            await BotHelper.SendTextMessageAsync(
                chatId, successMessage,
                botClient, logger, cancellationToken);
            await MenuManager.ShowMainMenuAsync(user, chatId, botClient, logger, cancellationToken);

            // send request with botuser to userbot to launch him separately
        }
    }
}
