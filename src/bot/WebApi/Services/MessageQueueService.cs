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
                    try
                    {
                        await ProcessJobAsync(request, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send requested message. Unknown error.");
                    }
                }
                await Task.Delay(3000, stoppingToken);
            }

            logger.LogInformation("Job Processor stopped");
        }

        private async Task ProcessJobAsync(SendMessageRequest request, CancellationToken cancellationToken)
        {
            string text = CompileFinalText(request);
            using var scope = serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetService<ITelegramBotClient>()!;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await botClient.SendMessage(request.ForumId, text, ParseMode.MarkdownV2, messageThreadId: (int)request.TopicId != 1 ? (int)request.TopicId : null, cancellationToken: cancellationToken);
                    return;
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 429)
                {
                    logger.LogError(ex, "429 Error sending message to forum topic. Seconds bot must to wait: {Wait}", ex.Parameters?.RetryAfter is not null ? ex.Parameters!.RetryAfter! : "I don't know");
                    await Task.Delay(ex.Parameters?.RetryAfter is not null ? ex.Parameters.RetryAfter.Value * 1000 : 3000, cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending message to forum topic.");
                    await Task.Delay(3000, cancellationToken);
                    continue;
                }
            }
            logger.LogError("Failed to send requested message. Unknown error.");
        }

        private static string CompileFinalText(SendMessageRequest request)
        {
            string stressedSourceText = BotHelper.RemoveSpecialChars(request.SourceText);
            foreach (var kw in request.FoundKeywords)
                stressedSourceText = stressedSourceText.Replace(BotHelper.RemoveSpecialChars(kw), $"*{BotHelper.RemoveSpecialChars(kw.ToUpper())}*", StringComparison.InvariantCultureIgnoreCase);
            stressedSourceText = stressedSourceText.Replace("\n", "\n> ");

            string header = $"> {stressedSourceText}";
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
            return normalizedFinalText;
        }
    }
}
