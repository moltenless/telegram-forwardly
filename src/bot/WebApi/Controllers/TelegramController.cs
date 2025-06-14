﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public Task<HealthCheckResult> Health()
        {
            return Task.FromResult(HealthCheckResult.Healthy("healthy"));
        }
    }
}
