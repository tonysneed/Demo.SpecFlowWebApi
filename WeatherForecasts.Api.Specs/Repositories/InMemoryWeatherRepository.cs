using WeatherForecasts.Api.Repositories;

namespace WeatherForecasts.Api.Specs.Repositories;

public class InMemoryWeatherRepository : IWeatherRepository
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild"
    };

    private List<WeatherForecast> WeatherForecasts { get; set; }

    public InMemoryWeatherRepository()
    {
        WeatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Id = index,
            Date = new DateTime(2022, 1, 1),
            TemperatureC = 32,
            Summary = Summaries[index - 1]
        }).ToList();
    }

    public Task<IEnumerable<WeatherForecast>> GetWeathers() => Task.FromResult(WeatherForecasts.AsEnumerable());

    public Task AddWeathers(IEnumerable<WeatherForecast> weatherForecasts)
    {
        WeatherForecasts = weatherForecasts.ToList();
        return Task.CompletedTask;
    }

    public Task RemoveWeathers()
    {
        WeatherForecasts.Clear();
        return Task.CompletedTask;
    }
}