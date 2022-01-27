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

    public async Task<IEnumerable<WeatherForecast>> GetAsync()=>
        await _documentRepository.FindManyAsync();

    public async Task<WeatherForecast?> GetAsync(int id)=>
        await _documentRepository.FindOneAsync(e => e.Id == id);

    public async Task<WeatherForecast?> AddAsync(WeatherForecast entity)
    {
        var existing = await _documentRepository.FindOneAsync(e => e.Id == entity.Id);
        if (existing != null) return null;
        if (string.IsNullOrWhiteSpace(entity.ETag))
            entity.ETag = Guid.NewGuid().ToString();
        return await _documentRepository.InsertOneAsync(entity);
    }

    public async Task<WeatherForecast?> UpdateAsync(WeatherForecast entity)
    {
        var existing = await GetAsync(entity.Id);
        if (existing == null) return null;
        if (string.Compare(entity.ETag, existing.ETag, StringComparison.OrdinalIgnoreCase) != 0 )
            throw new ConcurrencyException();
        entity.ETag = Guid.NewGuid().ToString();
        return await _documentRepository.FindOneAndReplaceAsync(e => e.Id == entity.Id, entity);
    }

    public async Task<int> RemoveAsync(int id)=>
        await _documentRepository.DeleteOneAsync(e => e.Id == id);
}

public class ConcurrencyException : Exception { }