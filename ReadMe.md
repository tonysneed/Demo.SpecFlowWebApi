# SpecFlow Web API Demo

Demo of creating SpecFlow tests for a .NET 6 Web API that uses a database.

> **Prerequisites**:
> - [.NET Core SDK](https://dotnet.microsoft.com/download) (6.0 or greater)
> - [Docker Desktop](https://www.docker.com/products/docker-desktop)
> - MongoDB Docker: `docker run --name mongo -d -p 27017:27017 -v /tmp/mongo/data:/data/db mongo`
> - [MongoDB Client](https://robomongo.org/download):
>    - Download Robo 3T only.
>    - Add connection to localhost on port 27017.
> - SpecFlow plugin for [Visual Studio](https://docs.specflow.org/projects/getting-started/en/latest/GettingStarted/Step1.html) or [JetBrains Rider](https://docs.specflow.org/projects/specflow/en/latest/Rider/rider-installation.html). 

### Introduction

This demo shows how you can create SpecFlow tests for a .NET Web API that uses a database.
- It uses the default Weatherforecast controller that is created with a new .NET 6 Web API.
  - The sample has added a **repository** that stores `WeatherForecast` entities with an `Id` of type `int` in a MongoDb database, using the **URF.Core.Mongo** package to implement a generic repository interface.
  - The Web API contains an **appsettings.json** file with a `WeatherDatabaseSettings` section that has the `ConnectionString`, `DatabaseName` and `CollectionName`.
  - The MongoDb connection is configured using a `Services.AddMongoDbSettings` method from the **EventDriven.DependencyInjection.URF.Mongo** package that accepts `builder.Configuration`.
  - In order for the SpecFlow project to configure settings in the Web API project, you need to set `InternalsVisibleTo` to the SpecFlow project and make **Program.cs** public by adding a partial class.
- The SpecFlow project uses a test database that is separate from the one used by the Web API project.
  - It has its own **appsettings.json** file that overrides the one in the Web API project.
- The SpecFlow project has a `JsonFilesRepository` that reads files in a **json** folder that provide values that are expected to be returned by the Web API.
- The `Hook` class in the **Hooks** folder has a `RegisterServices` method that is called before each scenario to set up a test instance of the Web API that runs in-memory, as well as an HTTP client that invokes methods on the Web API.
  - A new `WebApplicationFactory<Program>` is configured to use the **appsettings.json** file in the SpecFlow project, so that the Web API connects to the test database.
  - Data is cleared from the test database before each scenario.
  - An HTTP client is created from the web app factory and registered with SpecFlow's dependency injection system using the `IObjectContainer` injected into the `Hooks` constructor.
  - The `JsonFilesRepository` reads files from the **json** folder and is then registered with SpecFlow DI.
- The `WeatherWebApiStepDefinitions` class accepts both `WebApplicationFactory<Program>`, `IWeatherRepository` and `JsonFilesRepository` to its constructor.
  - `GetAsync` is called on `HttpClient` using the supplied endpoint.
  - You can then check the returned status code in a subsequent step.
  - `Response.Content.ReadAsStringAsync` is called to retrieve the response content and compare it to the expected result from `JsonFilesRepository`.

### Steps

1. Create solution with Web API project using .NET 6.
   - Add `IWeatherRepository` interface.
    ```csharp
    public interface IWeatherRepository
    {
        Task<IEnumerable<WeatherForecast>> GetAsync();
        Task<WeatherForecast?> GetAsync(int id);
        Task<WeatherForecast?> AddAsync(WeatherForecast entity);
        Task<WeatherForecast?> UpdateAsync(WeatherForecast entity);
        Task<int> RemoveAsync(int id);
    }
    ```
    - Add an implementation, for example, for MongoDb.
      - For MongoDb add the following packages.
        - MongoDB.Driver
        - URF.Core.Mongo
        - EventDriven.DependencyInjection.URF.Mongo
    - Update the WeatherForecastController as follows:
    ```csharp
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
    ```
    - In **Program.cs** add the following service registrations:
    ```csharp
    builder.Services.AddSingleton<IWeatherRepository, MongoWeatherRepository>();
    builder.Services.AddMongoDbSettings<WeatherDatabaseSettings, WeatherForecast>(builder.Configuration);
    ```
    - Add the following to **appsettings.json**:
    ```json
    "WeatherDatabaseSettings": {
      "ConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "WeathersDb",
      "CollectionName": "Weathers"
    }
    ```
2. Make `Program` in the Web API project visible to the SpecFlow project.
   - Add the following to the **.csproj** file.
    ```xml
    <ItemGroup>
        <InternalsVisibleTo Include="SpecFlowWebApi.Specs" />
    </ItemGroup>
    ```
   - Add the following to the end of **Program.cs**:
    ```csharp
    public partial class Program { }
    ```
3. Add SpecFlow project.
   - Reference Web API project.
   - Add the following packages:
     - Microsoft.AspNetCore.Mvc.Testing
4. Add **appsettings.json** file to the SpecFlow project.
   - Add the same content as in the Web API project.
     - Except set `DatabaseName` to `WeathersTestDb`.
   - On file properties set `Copy to output directory` to `Always`.
5. Add a **json** folder with a **weathers.json** file in it.
   - Paste json from executing GET on `weatherforecast` using Swagger.
   - Also add a **weather.json** file.
    ```json
    {
      "id": 1,
      "date": "2022-01-01T06:00:00Z",
      "temperatureC": 32,
      "temperatureF": 89,
      "summary": "Freezing",
      "eTag": "6e9eff61-a3ed-4339-93fb-24151149b46c"
    }
    ```
6. Add a **Repositories** folder with `InMemoryWeatherRepository`.
   - Use `List<WeatherForecast>` for the data.
   - Add a `JsonFilesRepository` class for reading files from the **json** folder.
    ```csharp
    public class JsonFilesRepository
    {
        private const string Root = "../../../json/";
        public Dictionary<string, string> Files { get; } = new();

        public JsonFilesRepository(params string[] files)
        {
            foreach (var file in files)
            {
                var path = Path.Combine(Root, file);
                var contents = File.ReadAllText(path);
                Files.Add(file, contents);
            }
        }
    }
    ```
7. Add a feature file to the SpecFlow project.
   - Add the following scenarios:
    ```
    Scenario: Get weather forecast by id
        Given I am a client
        And the repository has weather data
        When I make a GET request with id '1' to 'weatherforecast'
        Then the response status code is '200'
        And the response json should be 'weather.json'

    Scenario: Add weather forecast
        Given I am a client
        When I make a POST request with 'weather.json' to 'weatherforecast'
        Then the response status code is '201'
        And the location header is 'http://localhost/WeatherForecast/1'
        And the response json should be 'weather.json'

    Scenario: Update weather forecast
        Given I am a client
        And the repository has weather data
        When I make a PUT request with 'weather.json' to 'weatherforecast'
        Then the response status code is '200'
        And the response entity should be 'weather.json'
        And it should have a new ETag
        
    Scenario: Remove weather forecast
        Given I am a client
        And the repository has weather data
        When I make a DELETE request with id '1' to 'weatherforecast'
        Then the response status code is '204'
    ```
8.  Create a step for each line in this file.
   - Add a ctor that accepts `HttpClient` and `JsonFilesRepository`.
   - Implement the GET request step as follows:
        ```csharp
        [When(@"I make a GET request to '(.*)'")]
        public async Task WhenIMakeAgetRequestTo(string endpoint)
        {
            Response = await Client.GetAsync(endpoint);
        }
        ```
    - Implement the response status code step:
        ```csharp
        [Then(@"the response status code is '(.*)'")]
        public void ThenTheResponseStatusCodeIs(int statusCode)
        {
            var expected = (HttpStatusCode)statusCode;
            Assert.Equal(expected, Response.StatusCode);
        }
        ```
    - Implement the response data step:
        ```csharp
        [Then(@"the response data should be '(.*)'")]
        public async Task ThenTheResponseDataShouldBe(string file)
        {
            var expected = JsonFilesRepository.Files[file];
            var response = await Response.Content.ReadAsStringAsync();
            var actual = response.JsonPrettify();        
            Assert.Equal(expected, actual);
        }
        ```
9.  Lastly, flesh out the **Hook.cs** file in the **Hooks** folder.
    - Add a private `GetWebApplicationFactory` method that returns `WebApplicationFactory<Program>`.
      - Add code that adds the appsettings.json file to config and configures test services to bind it to `WeatherDatabaseSettings`.
    ```csharp
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
    ```
    - Add a private `ClearData` method that removes data from the test database. 
    ```csharp
    private async Task ClearData(
        WebApplicationFactory<Program> factory)
    {
        if (factory.Services.GetService(typeof(IWeatherRepository)) 
            is not IWeatherRepository repository) return;
        var entities = await repository.GetAsync();
        foreach (var entity in entities)
            await repository.RemoveAsync(entity.Id);
    }
    ```
    - Add a public `RegisterServices` method with a `[BeforeScenario]` hook that calls these two private methods.
      - Then uses the web application factory to create a default client with a specified base address.
      - Then registers both the client and the json repository with the injected `IObjectContainer`.
    ```csharp
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
    ```
