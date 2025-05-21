using Microsoft.AspNetCore.Mvc;
using TelegramForwardly.DataAccess.Entities;
using TelegramForwardly.DataAccess.Repositories.Interfaces;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TypesController(ILogger<TypesController> logger, ITopicGroupingTypeRepository topicGroupingTypeRepository)
        : ControllerBase
    {
        [HttpGet(Name = "GetTypes")]
        public async Task<ActionResult<IEnumerable<TopicGroupingType>>> GetAll()
        {
            var types = await topicGroupingTypeRepository.GetAllAsync();
            logger.LogInformation("Types are: {Message}", [.. types]);
            return Ok(types);
        }
    }
}
