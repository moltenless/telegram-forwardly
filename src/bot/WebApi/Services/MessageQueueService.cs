using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Services.Bot;

namespace TelegramForwardly.WebApi.Services
{
    public class MessageQueueService(
        ConcurrentQueue<SendMessageRequest> messageQueue,
        IServiceProvider serviceProvider,
        ILogger<MessageQueueService> logger
        ) : BackgroundService
    {
        private readonly ConcurrentQueue<SendMessageRequest> messageQueue = messageQueue;
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<MessageQueueService> logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Send message job processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (messageQueue.TryDequeue(out var request))
                {
                    logger.LogInformation("Processing job: {Start}",
                        request.SourceText[..Math.Min(10, request.SourceText.Length)]);
                    await ProcessJobAsync(request);
                }
                await Task.Delay(3000, stoppingToken);
            }

            logger.LogInformation("Job Processor stopped");
        }

        private async Task ProcessJobAsync(SendMessageRequest request)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var botClient = scope.ServiceProvider.GetService<ITelegramBotClient>()!;
                try
                {
                    string stressedSourceText = BotHelper.RemoveSpecialChars(request.SourceText);
                    foreach (var kw in request.FoundKeywords)
                        stressedSourceText = stressedSourceText.Replace(BotHelper.RemoveSpecialChars(kw), $"*{BotHelper.RemoveSpecialChars(kw.ToUpper())}*");
                    stressedSourceText = stressedSourceText.Replace("\n", "\n> ");

                    string header = $"Found:\n> {stressedSourceText}";
                    string footer = $"\n- *Detected keywords: +* {string.Join(", *+* ", request.FoundKeywords.Select(BotHelper.RemoveSpecialChars))}\n" +
                                        $"- *Message by*: {BotHelper.RemoveSpecialChars(request.SenderFirstName is not null ? request.SenderFirstName : "")}" +
                                        $" {((request.SenderUsername is not null) ? ("@" + request.SenderUsername) : $"tg://user?id={request.SenderId}")} \n" +
                                        $"- *Link to message*: [{BotHelper.RemoveSpecialChars(request.SourceChatTitle[..Math.Min(request.SourceChatTitle.Length, 25)])}]" +
                                        $"(https://t.me/c/{request.SourceChatId}/{request.SourceMessageId}) {request.DateTime}";

                    string finalText;
                    int lengthDelta = (header + footer).Length - 4096;
                    if (lengthDelta <= 0)
                        finalText = header + footer;
                    else
                        finalText = header[..Math.Min(header.Length, header.Length - lengthDelta - 3)] + "..." + footer;

                    string normalizedFinalText = BotHelper.EscapeMarkdownV2InTopic(finalText);

                    await botClient.SendMessage(request.ForumId, normalizedFinalText,
                        ParseMode.MarkdownV2, messageThreadId: (int)request.TopicId);

                    logger.LogInformation("Bot's been requested to forward and it sent the message to forum: {Forum} topic: {Topic}", request.ForumId, request.TopicId);
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 429)
                {
                    logger.LogError(ex, "ВНУТРИ АПИ ЕКПСПЕПШН НО КОГДА 429 Error sending message to forum topic.");
                }
                catch (ApiRequestException ex)
                {
                    logger.LogError(ex, "ВНУТРИ АПИ ЕКСЕПШН Error sending message to forum topic.");
                    await BotHelper.SendTextMessageAsync(request.ForumOwnerId,
                                $"An error occurred while sending filtered message to your forum topic. Here is details: {ex.Message}",
                                botClient, logger, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ВНУТРИ ОБЩЕГО Error sending message to forum topic.");
                    await BotHelper.SendTextMessageAsync(request.ForumOwnerId,
                                $"An error occurred while sending filtered message to your forum topic. Here is details: {ex.Message}",
                                botClient, logger, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting injected telegramBotClient");
            }
        }
    }
}
