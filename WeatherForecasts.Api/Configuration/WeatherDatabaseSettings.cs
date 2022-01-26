using EventDriven.DependencyInjection.URF.Mongo;

namespace WeatherForecasts.Api.Configuration;

public class WeatherDatabaseSettings : IMongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}