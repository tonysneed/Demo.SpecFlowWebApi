namespace WeatherForecasts.Api.Repositories;

public interface IWeatherRepository
{
    Task<IEnumerable<WeatherForecast>> GetWeathers();
    Task AddWeathers(IEnumerable<WeatherForecast> weatherForecasts);
    Task RemoveWeathers();
}