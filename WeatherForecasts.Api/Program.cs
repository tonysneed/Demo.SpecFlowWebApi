using EventDriven.DependencyInjection.URF.Mongo;
using WeatherForecasts.Api;
using WeatherForecasts.Api.Configuration;
using WeatherForecasts.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repository
builder.Services.AddSingleton<IWeatherRepository, MongoWeatherRepository>();
builder.Services.AddMongoDbSettings<WeatherDatabaseSettings, WeatherForecast>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }