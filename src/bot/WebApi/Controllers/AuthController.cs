using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramForwardly.WebApi.Services;
using TelegramForwardly.WebApi.Services.Bot;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IServiceProvider serviceProvider,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly ILogger<AuthController> logger = logger;

        [HttpGet("code")]
        public async Task<IActionResult> GetCodeEndpoint([FromQuery] long userId, [FromQuery] long chatId)
        {
            ITelegramBotClient botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            IUserService userService = serviceProvider.GetRequiredService<IUserService>();

            string code = null!;
            try
            {
                code = await AuthenticationManager.RequestAndAwaitVerificationCodeAsync(
                    userId, chatId, userService, botClient, logger, CancellationToken.None);
            }
            catch (TimeoutException te)
            {
                //send to user maybe
                logger.LogError(te, "Timeout while waiting for verification code for user {UserId}", userId);
            }
            return Ok(new { code });
        }
    }
}
