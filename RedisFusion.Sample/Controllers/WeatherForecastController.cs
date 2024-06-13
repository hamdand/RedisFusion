using Microsoft.AspNetCore.Mvc;
using RedisFusion.Services;
using RedisFusion.Utilities;

namespace RedisFusion.Sample.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRedisFusionService _redisService;

        public WeatherForecastController(IRedisFusionService redisService, ILogger<WeatherForecastController> logger)
        {
            _redisService = redisService;
            _logger = logger;
        }

        [HttpGet]
        [RedisFusionOutputCache]
        public async Task<IEnumerable<WeatherForecast>> GetAsAttribute()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [RedisFusionOutputCache(20)]
        public async Task<IEnumerable<WeatherForecast>> GetAsAttributeForDuration()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetOrAddService()
        {
            //var result = await _redisService.GetOrAddCachedObjectAsync("testKey", getData); //SAMPLE withour duration

            var result = await _redisService.GetOrAddCachedObjectAsync("testKey", getData, 20);

            return result;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetOnlyService()
        {
            var result = await _redisService.GetCachedObjectAsync<IEnumerable<WeatherForecast>>("testKey");

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        private async Task<IEnumerable<WeatherForecast>> getData()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
