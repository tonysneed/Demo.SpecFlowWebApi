using BoDi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecasts.Api.Configuration;
using WeatherForecasts.Api.Repositories;
using WeatherForecasts.Api.Specs.Repositories;

namespace WeatherForecasts.Api.Specs.Hooks
{
    [Binding]
    public class WeatherHooks
    {
        private readonly IObjectContainer _objectContainer;
        private const string AppSettingsFile = "appsettings.json";

        public WeatherHooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario]
        public async Task RegisterServices()
        {
            var factory = GetWebApplicationFactory();
            await ClearData(factory);
            _objectContainer.RegisterInstanceAs(factory);
            var jsonFilesRepo = new JsonFilesRepository();
            _objectContainer.RegisterInstanceAs(jsonFilesRepo);
            var repository = (IWeatherRepository)factory.Services.GetService(typeof(IWeatherRepository))!;
            _objectContainer.RegisterInstanceAs(repository);
        }

        private WebApplicationFactory<Program> GetWebApplicationFactory() =>
            new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    IConfigurationSection? configSection = null;
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), AppSettingsFile));
                        configSection = context.Configuration.GetSection(nameof(WeatherDatabaseSettings));
                    });
                    builder.ConfigureTestServices(services =>
                        services.Configure<WeatherDatabaseSettings>(configSection));
                });

        private async Task ClearData(
            WebApplicationFactory<Program> factory)
        {
            if (factory.Services.GetService(typeof(IWeatherRepository)) 
                is not IWeatherRepository repository) return;
            var entities = await repository.GetAsync();
            foreach (var entity in entities)
                await repository.RemoveAsync(entity.Id);
        }
    }
}