using System;
using System.IO;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Services.Bot;
using TelegramForwardly.WebApi.Services.Bot.Managers;
using TelegramForwardly.WebApi.Services.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task HandleUpdateAsync(Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update: {UpdateId}", update.Id);
            await HandleErrorAsync(update, ex, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
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

        ///////////////////////////////////////////////////////////////
        await userService.UpdateUserDateAsync(user.TelegramUserId);


        ////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        await userService.UpdateUserDateAsync(user.TelegramUserId);
        //////////////////////////////////////////////////////////////////////

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

    public async Task SendMessageAsync(SendMessageRequest request)
    {
        string header = $"{BotHelper.RemoveSpecialChars(request.SourceText)}";
        string footer = $"\n\nLink to message: https://t.me/c/{request.SourceChatId}/{request.SourceMessageId}\n" +
                          $"Detected keywords: {BotHelper.RemoveSpecialChars(string.Join(", ", request.FoundKeywords))}\n" +
                          $"From chat: {BotHelper.RemoveSpecialChars(request.SourceChatTitle[..Math.Min(request.SourceChatTitle.Length, 25)])}\n" +
                          $"Message by: {BotHelper.RemoveSpecialChars(request.SenderFirstName is not null ? request.SenderFirstName : "")}" +
                          $" {((request.SenderUsername is not null) ? ("@" + request.SenderUsername) : string.Empty)}\n" +
                          $"Time: {request.DateTime}";

        string finalText;
        int lengthDelta = (header + footer).Length - 4096;
        if (lengthDelta <= 0)
            finalText = header + footer;
        else
            finalText = header[..Math.Min(header.Length, header.Length - lengthDelta - 3)] + "..." + footer;

        string normalizedFinalText = BotHelper.EscapeMarkdownV2InTopic(finalText);
        try
        {
            await botClient.SendMessage(request.ForumId, normalizedFinalText, ParseMode.MarkdownV2, messageThreadId: (int)request.TopicId);
            logger.LogInformation("Bot's been requested and it sent the message to forum: {Forum} topic: {Topic}", request.ForumId, request.TopicId);
        }
        catch (ApiRequestException ex) when (ex.ErrorCode == 429)
        {
            logger.LogError(ex, "ВНУТРИ АПИ ЕКПСПЕПШН НО КОГДА 429 Error sending message to forum topic.\n\nReal message caused the problem:\n{NormalizedMessage}", normalizedFinalText);
            throw;
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "ВНУТРИ АПИ ЕКСЕПШН Error sending message to forum topic.\n\nReal message caused the problem:\n{NormalizedMessage}", normalizedFinalText);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ВНУТРИ ОБЩЕГО Error sending message to forum topic.\n\nReal message caused the problem:\n{NormalizedMessage}", normalizedFinalText);
            //await BotHelper.SendTextMessageAsync(userId,
            //            $"An error occurred while sending filtered message to your forum topic. Here is details: {ex.Message}",
            //            botClient, logger, CancellationToken.None);
            throw;
        }
    }
}
