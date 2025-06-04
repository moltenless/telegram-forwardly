using Microsoft.AspNetCore.Mvc;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        IUserService userService,
        ILogger<UsersController> logger) : ControllerBase
    {
        private readonly IUserService userService = userService;
        private readonly ILogger<UsersController> logger = logger;

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all/authenticated")]
        public async Task<IActionResult> GetAllAuthenticatedUsers()
        {
            try
            {
                var user = await userService.GetAllAuthenticatedUsersAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all authenticated users");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
