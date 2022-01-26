using System.Net;
using WeatherForecasts.Api.Specs.Helpers;
using WeatherForecasts.Api.Specs.Repositories;
using Xunit;

namespace WeatherForecasts.Api.Specs.Steps;

[Binding]
public class WeatherWebApiStepDefinitions
{
    private HttpClient Client { get; }
    public JsonRepository JsonRepository { get; }
    private HttpResponseMessage Response { get; set; } = null!;

    public WeatherWebApiStepDefinitions(
        HttpClient client,
        JsonRepository jsonRepository)
    {
        Client = client;
        JsonRepository = jsonRepository;
    }

    [Given(@"I am a client")]
    public void GivenIAmAClient()
    {
    }

    [When(@"I make a GET request to '(.*)'")]
    public async Task WhenIMakeAgetRequestTo(string endpoint)
    {
        Response = await Client.GetAsync(endpoint);
    }

    [Then(@"the response status code is '(.*)'")]
    public void ThenTheResponseStatusCodeIs(int statusCode)
    {
        var expected = (HttpStatusCode)statusCode;
        Assert.Equal(expected, Response.StatusCode);
    }

    [Then(@"the response data should be '(.*)'")]
    public async Task ThenTheResponseDataShouldBe(string file)
    {
        var expected = JsonRepository.Files[file];
        var response = await Response.Content.ReadAsStringAsync();
        var actual = response.JsonPrettify();        
        Assert.Equal(expected, actual);
    }
}