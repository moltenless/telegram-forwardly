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
        ILogger<MessageController> logger) : ControllerBase
    {
        private readonly IBotService botService = botService;
        private readonly ILogger<MessageController> logger = logger;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                await botService.SendMessageAsync(request);
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
