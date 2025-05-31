using Telegram.Bot;
using Telegram.Bot.Types;
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
            CancellationToken cancellationToken
            )
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
            CancellationToken cancellationToken
            )
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
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            string apiHash = message.Text?.Trim() ?? string.Empty;
            await userService.UpdateUserApiHashAsync(user.TelegramUserId, apiHash);
            string apiId = user.ApiId!;
            string phone = user.Phone!;

            var authResult = await userbotApiService.StartAuthenticationAsync(
                user.TelegramUserId, phone, apiId, apiHash);

            if (!authResult.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Authentication failed: {authResult.ErrorMessage}\nPlease try again with /setup", 
                    botClient, logger,
                    cancellationToken);
                await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingVerificationCode);

            await BotHelper.SendTextMessageAsync(
                message.Chat.Id,
                "A verification code has been sent to your Telegram account. *Please send me the code:*",
                botClient, logger,
                cancellationToken);
        }
    }
}
