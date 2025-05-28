using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController(
        IServiceProvider serviceProvider,
        ILogger<TelegramController> logger) : ControllerBase
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<TelegramController> logger = logger;

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] Update update, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Received update: {UpdateId}", update.Id);

                using var scope = serviceProvider.CreateScope();
                var botService = scope.ServiceProvider.GetRequiredService<IBotService>();

                await botService.HandleUpdateAsync(update, cancellationToken);

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling webhook update {UpdateId}", update.Id);
                return StatusCode(500);
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
