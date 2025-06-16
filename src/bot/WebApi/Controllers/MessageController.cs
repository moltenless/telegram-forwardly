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
                {
                    logger.LogInformation("ignored, not enqueued");
                    return Ok();
                }

                int userMessagesCountInQueue = messageQueue.Count(r => r.ForumOwnerId == request.ForumOwnerId);
                if (userMessagesCountInQueue >= 20)  // these constants are strongly dependent on total bot supported user amount
                {
                    logger.LogWarning("Too many message requests for user {User} in general queue. The request will be truncated", request.ForumOwnerId);
                    using var scope = serviceProvider.CreateScope();
                    ITelegramBotClient botClient = scope.ServiceProvider.GetService<ITelegramBotClient>()!;
                    await BotHelper.SendTextMessageAsync(request.ForumOwnerId, "⚠️ Too many of your messages are being forwarded at once right now. " +
                        "*Forwarding has been paused for one minute*. Please refine your keywords to receive only relevant forwards.", botClient, logger, CancellationToken.None);

                    if (queueOverloadLastTimes.ContainsKey(request.ForumOwnerId))
                        queueOverloadLastTimes[request.ForumOwnerId] = DateTime.UtcNow;
                    else
                        queueOverloadLastTimes.Add(request.ForumOwnerId, DateTime.UtcNow);

                    //IUserbotApiService userbotApiService = scope.ServiceProvider.GetService<IUserbotApiService>()!;
                    //logger.LogWarning("disabling has been requested");
                    //FieldUpdateResult result = await userbotApiService.UpdateForwardlyEnabledAsync(request.ForumOwnerId, false);
                    //logger.LogWarning("disabling completed!!!!!!");
                    //if (!result.Success) throw new HttpRequestException("\n\nCRITICAL ERROR. Bot couldn't disable forwarding in userbot of user who has reached message limit in queue.\n\n");
                    //IUserService userService = scope.ServiceProvider.GetService<IUserService>()!;
                    //await userService.ToggleForwardlyEnabledAsync(request.ForumOwnerId, false);

                    //await Task.Delay(60_000);

                    //FieldUpdateResult result2 = await userbotApiService.UpdateForwardlyEnabledAsync(request.ForumOwnerId, true);
                    //if (!result2.Success) throw new HttpRequestException("\n\nCRITICAL ERROR. Bot couldn't enable again forwarding in userbot of user who has reached message limit in queue.\n\n");
                    //await userService.ToggleForwardlyEnabledAsync(request.ForumOwnerId, true);

                    return Ok();
                }
                if (messageQueue.Count >= 100) // this condition hopefully never gets satisfied
                {
                    logger.LogWarning("\n\nCRITICAL ERROR. Too many total message requests for all users in general queue. The number exceeds 100. The request will be truncated\n\n");
                    return Ok();
                }

                messageQueue.Enqueue(request);
                logger.LogInformation("message send request has been enqueued. Queue total size is {Size}", messageQueue.Count);
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
