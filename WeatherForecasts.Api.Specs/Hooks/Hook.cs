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
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private const string BaseAddress = "http://localhost/";
        private const string AppSettingsFile = "appsettings.json";
        public string[] Files { get; } = { "weathers.json" };

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario]
        public async Task RegisterServices()
        {
            var factory = GetWebApplicationFactory();
            await EnsureDatabase(factory);
            var client = factory.CreateDefaultClient(new Uri(BaseAddress));
            _objectContainer.RegisterInstanceAs(client);
            var jsonRepo = new JsonRepository(Files);
            _objectContainer.RegisterInstanceAs(jsonRepo);
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

        private async Task EnsureDatabase(WebApplicationFactory<Program> factory)
        {
            if (factory.Services.GetService(typeof(IWeatherRepository)) 
                is not IWeatherRepository weatherRepository) return;
            await weatherRepository.RemoveWeathers();
            var weathers = await new InMemoryWeatherRepository().GetWeathers();
            await weatherRepository.AddWeathers(weathers);
        }
    }
}