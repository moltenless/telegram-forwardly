using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Bot.Managers;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IServiceProvider serviceProvider,
        IOptions<TelegramConfig> config,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly string apiKey = config.Value.ApiKey;
        private readonly ILogger<AuthController> logger = logger;

        [HttpGet("code")]
        public async Task<IActionResult> GetCodeEndpoint([FromQuery] long userId, [FromQuery] long chatId, [FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != this.apiKey)
            {
                return Unauthorized("Invalid or missing API key");
            }

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
                //send to user - maybe
                logger.LogError(te, "Timeout while waiting for verification code for user {UserId}", userId);
            }
            return Ok(new { code });
        }
    }
}
