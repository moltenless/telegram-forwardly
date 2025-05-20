using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TelegramForwardly.DataAccess.Context;
using TelegramForwardly.DataAccess.Entities;

namespace TelegramForwardly.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        private ForwardlyContext context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ForwardlyContext forwardlyContext)
        {
            _logger = logger;
            context = forwardlyContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public WeatherForecast Get()
        {
            TopicGroupingType type = new TopicGroupingType { Value = "Test" };
            context.TopicGroupingTypes.Add(type);
            string message = "added";
            context.SaveChanges();
            return new WeatherForecast
            {
                Message = message
            };
        }
    }
}
