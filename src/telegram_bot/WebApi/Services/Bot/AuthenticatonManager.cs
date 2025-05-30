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
    }
}
