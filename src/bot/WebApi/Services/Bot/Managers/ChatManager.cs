using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class ChatManager
    {
        public static async Task HandleAllChatsToggleAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            string input = message.Text!.Trim().ToLower();
            bool? enableAllChats = null;
            if (input == "all") enableAllChats = true;
            else if (input == "next") enableAllChats = false;


            if (enableAllChats is null)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Invalid input. Please send either 'next' or 'all'.",
                    botClient, logger, cancellationToken);
                return;
            }

            FieldUpdateResult result = await userbotApiService.SetAllChatsEnabledAsync(
                user.TelegramUserId, (bool)enableAllChats);

            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                     message.Chat.Id,
                     $"Failed to update all chats setting: {result.ErrorMessage}",
                     botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            await userService.SetUserAllChatsEnabledAsync(user.TelegramUserId, (bool)enableAllChats);

            // After updating all chats, manage the selected ones

            HashSet<Models.Dtos.Chat> chats = await userService.GetUserChatsAsync(user.TelegramUserId);
            string chatList = "Selected chats to automatically check for keywords:\n\n";
            chatList += chats.Count != 0
                ? string.Join("\n", chats.Select(c => c.Title))
                : "No chats selected yet.";

            var keyboard = new InlineKeyboardMarkup(
            [
                [ InlineKeyboardButton.WithCallbackData("➕ Add Chat", "add_chat"),
                    InlineKeyboardButton.WithCallbackData("🗑️ Remove Chat", "remove_chat") ],
                [ InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu") ]
            ]);


            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, chatList, botClient, 
                logger, cancellationToken, replyMarkup: keyboard);

            ////////// Is it that we need to set the user state to AwaitingChats?
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingChats);
        }
    }
}
