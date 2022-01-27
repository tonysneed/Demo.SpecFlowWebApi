namespace WeatherForecasts.Api.Repositories;

public interface IWeatherRepository
{
    Task<IEnumerable<WeatherForecast>> GetAsync();
    Task<WeatherForecast?> GetAsync(int id);
    Task<WeatherForecast?> AddAsync(WeatherForecast entity);
    Task<WeatherForecast?> UpdateAsync(WeatherForecast entity);
    Task<int> RemoveAsync(int id);
}