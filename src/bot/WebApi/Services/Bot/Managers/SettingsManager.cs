using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class SettingsManager
    {
        public static async Task HandleTopicGroupIdAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            long forumId = long.Parse(message.Text!);

            FieldUpdateResult result = await userbotApiService.UpdateUserForumAsync(user.TelegramUserId, forumId);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to set forum/group ID: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            await userService.UpdateUserForumIdAsync(user.TelegramUserId, forumId);

            await BotHelper.SendTextMessageAsync(message.Chat.Id, "Set grouping mode:", botClient, logger, cancellationToken);
            await MenuManager.EnterSettingsAsync(false, message, botClient, logger, cancellationToken);
        }

        public static async Task HandleGroupingTypeInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            GroupingMode mode;
            string response;
            if (message.Text == "2")
            {
                mode = GroupingMode.ByKeyword;
                response = "You have selected grouping by keywords.";
            }
            else if (message.Text == "1")
            {
                mode = GroupingMode.ByChat;
                response = "You have selected grouping by chat titles.";
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    "Invalid input. Please send '1' for chat titles grouping or '2' for keywords grouping.",
                    botClient, logger, cancellationToken);
                return;
            }

            var result = await userbotApiService.UpdateUserGroupingAsync(user.TelegramUserId, mode);

            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to set grouping mode: {result.ErrorMessage}",
                    botClient, logger, cancellationToken, parseMode: ParseMode.None);
                return;
            }

            await userService.SetUserGroupingModeAsync(user.TelegramUserId, mode);
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);

            response += user.Chats.Count == 0 ?
                "\n\n_Now it is time to choose from what chats you want to extract messages with specific keywords_:\n" +
               "*Please click '💬 Chats' in menu below*\nOr\n*Use /chats*\n_There you can also enable tracking of incoming messages from all of your chats_" : string.Empty;
            await BotHelper.SendTextMessageAsync(
                message.Chat.Id, response,
                botClient, logger, cancellationToken);

            await MenuManager.ShowMainMenuAsync(
                user, message.Chat.Id,
                botClient, logger, cancellationToken);
        }


        public static async Task HandleBotJoinedGroupAsync(
            Telegram.Bot.Types.Chat chat,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            try
            {
                if (chat.Type == ChatType.Supergroup && chat.IsForum)
                {
                    var message = $"🤖 Bot successfully added to forum!\n\n" +
                     $"📋 Copy this forum ID: `{chat.Id}` and paste it back to the chat with 'Forwardly' bot";
                    await BotHelper.SendTextMessageAsync(
                        chat.Id, message, botClient,
                        logger, cancellationToken);
                }
                else
                {
                    await BotHelper.SendTextMessageAsync(
                        chat.Id, "Bot added but this group doesn't seem to be forum group with topics enabled.",
                        botClient, logger, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send welcome message to forum {ChatId}", chat.Id);
            }
        }


        public static async Task ToggleForwardlyAsync(
            bool editSourceMessage,
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            FieldUpdateResult result = await userbotApiService.UpdateForwardlyEnabledAsync(user.TelegramUserId, !user.ForwardlyEnabled!.Value);
            if (!result.Success)
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Failed to toggle forwarding: {result.ErrorMessage}",
                    botClient, logger, cancellationToken);
                return;
            }

            await userService.ToggleForwardlyEnabledAsync(user.TelegramUserId, !user.ForwardlyEnabled!.Value);
            user = await userService.GetUserAsync(user.TelegramUserId);

            if (editSourceMessage)
                await botClient.EditMessageText(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    text: BotHelper.DefaultEscapeMarkdownV2(BotHelper.GetMenuText(user)),
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: BotHelper.GetMenuKeyboard(user.ForwardlyEnabled!.Value),
                    cancellationToken: cancellationToken);
            else
                await MenuManager.ShowMainMenuAsync(
                    user, message.Chat.Id,
                    botClient, logger, cancellationToken);
        }
    }
}
