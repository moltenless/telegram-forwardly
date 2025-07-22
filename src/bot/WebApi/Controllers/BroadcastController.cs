using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Models.Requests;
using TelegramForwardly.WebApi.Services.Bot;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BroadcastController(
        IUserService userService,
        ITelegramBotClient botClient,
        IOptions<TelegramConfig> config,
        ILogger<BroadcastController> logger) : ControllerBase
    {
        private readonly string apiKey = config.Value.ApiKey;

        [HttpPost("notify")]
        public async Task<IActionResult> BroadcastNotificationAsync(
            [FromHeader(Name = "X-Api-Key")] string apiKey,
            [FromBody] BroadcastNotificationRequest request)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != this.apiKey)
                return Unauthorized("Invalid or missing API key");

            if (request is null) return BadRequest("Text is missing");

            try
            {
                var usersIds = (await userService.GetAllUsersAsync())
                    .Select(u => u.TelegramUserId);

                foreach (var id in usersIds)
                {
                    await BotHelper.SendTextMessageAsync(
                        id, request.Text, botClient, logger,
                        CancellationToken.None, null,
                        request.UseMarkdown ? ParseMode.MarkdownV2 : ParseMode.None);
                    logger.LogInformation("Broadcasted user {Id} with notification: {Text}", id, request.Text[0..10]); // can cause an error if notification is smaller
                }
                return Ok();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error broadcasting the notification");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}