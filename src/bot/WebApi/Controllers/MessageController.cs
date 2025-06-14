using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Telegram.Bot;
using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController(
        IBotService botService,
        ILogger<UsersController> logger) : ControllerBase
    {
        private readonly IBotService botService = botService;
        private readonly ILogger<UsersController> logger = logger;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                await botService.SendMessageAsync(request.UserId, request.ForumId, request.TopicId, request.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, $"Internal bot server error. Failed to send message to forum {ex.Message}");
            }
        }
    }
}
