namespace WeatherForecasts.Api.Specs.Helpers;

public class WeatherForecastComparer : IEqualityComparer<WeatherForecast>
{
    public bool Equals(WeatherForecast? x, WeatherForecast? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id && x.Date.Equals(y.Date) && x.TemperatureC == y.TemperatureC && x.Summary == y.Summary;
    }

    public int GetHashCode(WeatherForecast obj)
    {
        return HashCode.Combine(obj.Id, obj.Date, obj.TemperatureC, obj.Summary);
    }
}