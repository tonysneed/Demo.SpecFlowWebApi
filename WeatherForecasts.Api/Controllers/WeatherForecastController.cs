using Microsoft.AspNetCore.Mvc;
using WeatherForecasts.Api.Repositories;

namespace WeatherForecasts.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherRepository _weatherRepository;

    public WeatherForecastController(
        IWeatherRepository weatherRepository,
        ILogger<WeatherForecastController> logger)
    {
        _weatherRepository = weatherRepository;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get()
    {
        var result = await _weatherRepository.GetWeathers();
        return Ok(result.ToList());
    }
}