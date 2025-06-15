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
                messageQueue.Enqueue(request);
                logger.LogInformation("message send request has been enqueued");
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
