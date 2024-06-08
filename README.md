# RedisFusion

[![NuGet version](https://badge.fury.io/nu/RedisFusion.svg)](https://github.com/hamdand/RedisFusion)

## Overview

The `RedisFusion` package provides middleware and services to integrate Redis caching into your ASP.NET Core applications. It includes utilities for setting up Redis cache, a middleware for caching HTTP responses, and services to interact with Redis.

## Features

- Easy integration of Redis cache in ASP.NET Core applications.
- Middleware to cache HTTP responses.
- Services to interact with Redis for storing and retrieving data.

## Installation

To install the package, run the following command in the NuGet Package Manager Console:

```sh
dotnet add package RedisFusion
```

## Configuration

1. Add the Redis configuration in your `appsettings.json` file:

```json
"RedisFusionConfigurations": {
    "IsEnabled": true,
    "ConnectionString": "localhost:6379",
    "InstanceName": "MyCollection",
    "ExpirationTimeSpanInMinutes": 5
  }
```

2. Register the required services in your `Program.cs`:

```csharp
   // Register register RedisFusionService
   builder.Services.AddRedisFusionService(builder.Configuration);
   
   var app = builder.Build();
   
  app.UseRedisFusionOutputCache(); // Use Output Redis
   
   app.UseHttpsRedirection();
   ....
   
   app.Run();
   
```

   ## Usage

   ### Using Redis Fusion Middleware

   Apply the `OutputRedisCache` attribute to your controller methods to enable response caching:
   ```csharp
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

   ```


## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Contact

For any issues or feature requests, please open an issue on the GitHub repository.
