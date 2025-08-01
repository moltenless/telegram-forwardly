﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController(
        IServiceProvider serviceProvider,
        IOptions<TelegramConfig> config,
        ILogger<TelegramController> logger) : ControllerBase
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly string secretToken = config.Value.WebhookSecretToken;
        private readonly ILogger<TelegramController> logger = logger;

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] Update update,
            [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string secretToken,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(secretToken) || secretToken != this.secretToken)
                return BadRequest("Who are you? Webhook has been triggered but secret token was missing or invalid. But nice try :)");
            if (update == null || update.Type == 0)
                return BadRequest("Received null update payload in webhook. But nice try :)");
            try
            {
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
            return Task.FromResult(HealthCheckResult.Healthy("Healthy. Welcome, stranger!"));
        }
    }
}
