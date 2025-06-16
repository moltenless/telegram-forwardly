using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Models.Responses;
using TelegramForwardly.WebApi.Services;
using TelegramForwardly.WebApi.Services.Bot;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController(
        ConcurrentQueue<SendMessageRequest> messageQueue,
        IServiceProvider serviceProvider,
        Dictionary<long, DateTime> queueOverloadLastTimes,
        ILogger<MessageController> logger) : ControllerBase
    {
        private readonly ConcurrentQueue<SendMessageRequest> messageQueue = messageQueue;
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<MessageController> logger = logger;
        private readonly Dictionary<long, DateTime> queueOverloadLastTimes = queueOverloadLastTimes;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                if (queueOverloadLastTimes.TryGetValue(request.ForumOwnerId, out DateTime value)
                    && (DateTime.UtcNow - value).TotalSeconds <= 60)
                    return Ok();

                int userMessagesCountInQueue = messageQueue.Count(r => r.ForumOwnerId == request.ForumOwnerId);
                if (userMessagesCountInQueue >= 20)  // these constants are strongly dependent on total bot supported user amount
                {
                    using var scope = serviceProvider.CreateScope();
                    ITelegramBotClient botClient = scope.ServiceProvider.GetService<ITelegramBotClient>()!;
                    await BotHelper.SendTextMessageAsync(request.ForumOwnerId, "⚠️ Too many of your messages are being forwarded at once right now. " +
                        "*Forwarding has been paused for one minute*. Please refine your keywords to receive only relevant forwards.", botClient, logger, CancellationToken.None);

                    if (queueOverloadLastTimes.ContainsKey(request.ForumOwnerId))
                        queueOverloadLastTimes[request.ForumOwnerId] = DateTime.UtcNow;
                    else
                        queueOverloadLastTimes.Add(request.ForumOwnerId, DateTime.UtcNow);

                    return Ok();
                }
                if (messageQueue.Count >= 100) // this condition hopefully never gets satisfied
                {
                    logger.LogWarning("\n\nCRITICAL ERROR. Too many total message requests for all users in general queue. The number exceeds 100. The request will be truncated\n\n");
                    return Ok();
                }

                messageQueue.Enqueue(request);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending message to forum");
                return StatusCode(500, $"Internal bot server error. Failed to send message to forum {ex.Message}");
            }
        }
    }
}
