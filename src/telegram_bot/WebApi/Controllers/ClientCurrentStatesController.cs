using Microsoft.AspNetCore.Mvc;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientCurrentStatesController(ILogger<ClientCurrentStatesController> logger, IClientCurrentStatesRepository repository)
        : ControllerBase
    {
        [HttpGet(Name = "GetStates")]
        public async Task<ActionResult<IEnumerable<ClientCurrentState>>> GetAll()
        {
            var states = await repository.GetAllAsync();
            logger.LogInformation("states: {Message}", [.. states]);
            return Ok(states);
        }
    }
}
