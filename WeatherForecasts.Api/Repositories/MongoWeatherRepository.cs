using URF.Core.Abstractions;

namespace WeatherForecasts.Api.Repositories;

public class MongoWeatherRepository : IWeatherRepository
{
    private readonly IDocumentRepository<WeatherForecast> _documentRepository;

    public MongoWeatherRepository(
        IDocumentRepository<WeatherForecast> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeathers() =>
        await _documentRepository.FindManyAsync();

    public async Task AddWeathers(IEnumerable<WeatherForecast> weathers) =>
        await _documentRepository.InsertManyAsync(weathers);

    public async Task RemoveWeathers() =>
        await _documentRepository.DeleteManyAsync(_ => true);
}