using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.DataAccess.Repositories;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Bot;
using TelegramForwardly.WebApi.Services.Bot.Managers;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services;

public class BotService(
    ITelegramBotClient botClient,
    IUserService userService,
    IUserbotApiService userbotApiService,
    ILogger<BotService> logger
    ) : IBotService
{
    private readonly ITelegramBotClient botClient = botClient;
    private readonly IUserService userService = userService;
    private readonly IUserbotApiService userbotApiService = userbotApiService;
    private readonly ILogger logger = logger;

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(update, cancellationToken),
                UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery!, cancellationToken),
                _ => Task.CompletedTask
            };

            await handler;
        }
        catch (ClientCreationDeniedException ex)
        {
            long userId = update.Type switch
            {
                UpdateType.Message => update.Message!.From!.Id,
                UpdateType.CallbackQuery => update.CallbackQuery!.From!.Id,
                _ => -1
            };
            logger.LogError(ex, "Client {Id} couldn't be created because all supported by bot clients are already registered", userId);
            if (userId != -1)
                await BotHelper.SendTextMessageAsync(userId, "You can't use this bot because it has reached the limit of supported users. " +
                    "Please contact @moltenless to request access.", botClient, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update: {UpdateId}", update.Id);
            await HandleErrorAsync(update, ex, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
    {
        Message message = update.Message!;
        if (message.Type == MessageType.NewChatMembers && message.NewChatMembers![0].Id == botClient.BotId)
        {
            await SettingsManager.HandleBotJoinedGroupAsync(
                message.Chat, botClient, logger, cancellationToken);
            return;
        }
        if (message.Type != MessageType.Text && message.Type != MessageType.Contact)
            return;

        var user = await userService.GetOrCreateUserAsync(
            message.From!.Id, UserState.Idle, message.From!.Username, message.From!.FirstName);

        var messageText = message.Text ?? string.Empty;

        if (messageText.StartsWith('/'))
        {
            await UpdateRouter.RouteCommandAsync(
                user, message,
                userService, userbotApiService,
                botClient, logger,
                cancellationToken);
            return;
        }

        await UpdateRouter.RouteUserInputAsync(
            user, message,
            userService, userbotApiService,
            botClient, logger,
            cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var user = await userService.GetOrCreateUserAsync(
            callbackQuery.From!.Id, UserState.Idle, callbackQuery.From!.Username, callbackQuery.From!.FirstName);

        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        await UpdateRouter.RouteCallbackQueryAsync(
            user, callbackQuery,
            userService, userbotApiService,
            botClient, logger,
            cancellationToken);
    }

    private async Task HandleErrorAsync(Telegram.Bot.Types.Update update, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An error occurred while handling update {UpdateId}", update.Id);

        if (update.Message != null)
        {
            try
            {
                await BotHelper.SendTextMessageAsync(update.Message.Chat.Id,
                    "An error occurred while processing your request. Please try again later.",
                    botClient, logger,
                    cancellationToken);
            }
            catch
            {
                // Ignore errors when sending error messages
            }
        }
    }
}
