namespace WeatherForecasts.Api.Specs.Repositories;

public class JsonRepository
{
    private const string Root = "../../../json/";
    public Dictionary<string, string> Files { get; } = new();

    public JsonRepository(params string[] files)
    {
        foreach (var file in files)
        {
            var path = Path.Combine(Root, file);
            var contents = File.ReadAllText(path);
            Files.Add(file, contents);
        }
    }
}