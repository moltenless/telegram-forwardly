using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using TelegramForwardly.WebApi.Models.Requests;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController(
        ConcurrentQueue<SendMessageRequest> messageQueue,
        ILogger<MessageController> logger) : ControllerBase
    {
        private readonly ConcurrentQueue<SendMessageRequest> messageQueue = messageQueue;
        private readonly ILogger<MessageController> logger = logger;

        [HttpPost("send")]
        public IActionResult SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                int userMessagesCountInQueue = messageQueue.Count(r => r.ForumOwnerId == request.ForumOwnerId);
                if (userMessagesCountInQueue >= 20)  // these constants are strongly dependent on total bot supported user amount
                {
                    logger.LogWarning("Too many message requests for user {User} in general queue. The request will be truncated", request.ForumOwnerId);
                    return Ok();
                }
                if (messageQueue.Count >= 100) // this condition hopefully never get satisfied
                {
                    logger.LogWarning("Too many total message requests for all users in general queue. The number exceeds 100. The request will be truncated");
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
