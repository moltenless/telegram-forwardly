using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        IUserService userService,
        IOptions<TelegramConfig> config,
        ILogger<UsersController> logger) : ControllerBase
    {
        private readonly IUserService userService = userService;
        private readonly string apiKey = config.Value.ApiKey;
        private readonly ILogger<UsersController> logger = logger;

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers([FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != this.apiKey)
            {
                return Unauthorized("Invalid or missing API key");
            }

            try
            {
                var users = await userService.GetAllUsersAsync();
                var json = JsonSerializer.Serialize(users);
                return new ContentResult
                {
                    Content = json,
                    ContentType = "application/json",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all/authenticated")]
        public async Task<IActionResult> GetAllAuthenticatedUsers([FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey != this.apiKey)
            {
                return Unauthorized("Invalid or missing API key");
            }
            
            try
            {
                var users = await userService.GetAllAuthenticatedUsersAsync();
                var json = JsonSerializer.Serialize(users);
                return new ContentResult
                {
                    Content = json,
                    ContentType = "application/json",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all authenticated users");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
