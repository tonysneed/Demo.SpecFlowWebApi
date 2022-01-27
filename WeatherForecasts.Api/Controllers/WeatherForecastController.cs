using Microsoft.AspNetCore.Mvc;
using WeatherForecasts.Api.Repositories;

namespace WeatherForecasts.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherRepository _repository;

    public WeatherForecastController(
        IWeatherRepository repository,
        ILogger<WeatherForecastController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _repository.GetAsync();
        return Ok(result.ToList());
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var result= await _repository.GetAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WeatherForecast entity)
    {
        var result= await _repository.AddAsync(entity);
        if (result == null)
        {
            _logger.LogError("Entity already exists with id '{Id}'", entity.Id);
            return Conflict();
        }
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] WeatherForecast entity)
    {
        try
        {
            var result= await _repository.UpdateAsync(entity);
            return Ok(result);
        }
        catch (ConcurrencyException)
        {
            _logger.LogError("Concurrency error with entity id '{Id}'", entity.Id);
            return Conflict();
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _repository.RemoveAsync(id);
        if (result > 0 ) return NoContent();
        return NotFound();
    }
}