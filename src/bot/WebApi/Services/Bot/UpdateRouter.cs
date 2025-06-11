using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Bot.Managers;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot
{
    public static class UpdateRouter
    {
        public static async Task RouteCommandAsync(BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
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

                case "/settings":
                    await MenuManager.EnterSettingsAsync(
                        user, message.Chat.Id,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "/chats":
                    await ChatManager.RunChatMenuAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "/keywords":
                    await KeywordManager.EnterKeywordsAsync(
                        user, message,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "/status":
                    break;

                case "/help":
                    break;

                case "/delete":
                    await MenuManager.AskToDeleteData(
                        user, message.Chat.Id,
                        userService, botClient, logger, cancellationToken);
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
            IUserbotApiService userbotApiService,
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

                case "settings":
                    await MenuManager.EnterSettingsAsync(
                        user, callbackQuery.Message!.Chat.Id,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "chats":
                    await ChatManager.RunChatMenuAsync(
                        user, callbackQuery.Message!, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "keywords":
                    await KeywordManager.EnterKeywordsAsync(
                        user, callbackQuery.Message!,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "status":
                    break;

                case "help":
                    break;

                case "chat_view_page_back":
                    await ChatManager.TurnViewPageBackAsync(
                        user, callbackQuery,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "chat_view_page_forward":
                    await ChatManager.TurnViewPageForwardAsync(
                        user, callbackQuery,
                        userService, botClient, logger, cancellationToken);
                    break;

                case "add_chat":
                    await ChatManager.StartChatAdditionAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "chat_addition_page_back":
                    await ChatManager.TurnAddPageBackAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "chat_addition_page_forward":
                    await ChatManager.TurnAddPageForwardAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "remove_chat":
                    await ChatManager.StartChatDeletionAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "chat_deletion_page_back":
                    await ChatManager.TurnDeletePageBackAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "chat_deletion_page_forward":
                    await ChatManager.TurnDeletePageForwardAsync(
                        user, callbackQuery, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case "enable_all_chats":
                    await ChatManager.AskToEnableAllChatsAsync(
                        user, callbackQuery.Message!, userService,
                        botClient, logger, cancellationToken);
                    break;

                case "back_to_menu":
                    await userService.SetUserStateAsync(user.TelegramUserId, UserState.Idle);
                    await botClient.EditMessageText(
                        chatId: callbackQuery.Message!.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        text: BotHelper.DefaultEscapeMarkdownV2(BotHelper.GetMenuText(user)),
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                        replyMarkup: BotHelper.GetMenuKeyboard(),
                        cancellationToken: cancellationToken);
                    break;

                case "back_to_chat_menu":
                    await botClient.DeleteMessage(
                        chatId: callbackQuery.Message!.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    await ChatManager.RunChatMenuAsync(
                        user, callbackQuery.Message!,
                        userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;
            }
        }

        public static async Task RouteUserInputAsync(
            BotUser user,
            Message message,
            IUserService userService,
            IUserbotApiService userbotApiService,
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
                        user, message, userService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingSessionString:
                    await AuthenticationManager.HandleSessionStringInputAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingEnableAllChats:
                    await ChatManager.HandleAllChatsToggleAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingChats:
                    await ChatManager.HandleChatSelectionAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingRemoveChats:
                    await ChatManager.HandleChatDeletionSelectionAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingKeywords:
                    break;

                case UserState.AwaitingForumGroup:
                    await SettingsManager.HandleTopicGroupId(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingGroupingType:
                    await SettingsManager.HandleGroupingTypeInputAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
                    break;

                case UserState.AwaitingDeleteConfirmation:
                    await AuthenticationManager.HandleDeleteConfirmationInputAsync(
                        user, message, userService, userbotApiService,
                        botClient, logger, cancellationToken);
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
