using System.Net.Http.Headers;
using GeoTrack.API.Middleware;
using GeoTrack.API.Specs.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reqnroll;
using Xunit;

namespace GeoTrack.API.Specs.StepDefinitions;

[Binding]
public sealed class ApiSteps
{
    private readonly GeoTrackApiFactory _factory;
    private readonly ScenarioContext _scenarioContext;

    public ApiSteps(GeoTrackApiFactory factory, ScenarioContext scenarioContext)
    {
        _factory = factory;
        _scenarioContext = scenarioContext;
    }

    private HttpClient Client
    {
        get
        {
            if (!_scenarioContext.TryGetValue(nameof(HttpClient), out HttpClient? client) || client is null)
            {
                client = _factory.CreateClient();
                _scenarioContext[nameof(HttpClient)] = client;
            }

            return client;
        }
    }

    [Given("the API is running")]
    public void GivenTheApiIsRunning()
    {
        // Creating the client boots the in-memory TestServer.
        _ = Client;
    }

    [Given("I have a valid API key")]
    public void GivenIHaveAValidApiKey()
    {
        var apiKey = _factory.Services
            .GetRequiredService<IOptions<ApiKeyOptions>>()
            .Value
            .Keys
            .First();

        // Replace any previous header value
        Client.DefaultRequestHeaders.Remove("X-Api-Key");
        Client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    [When("I GET {string}")]
    public async Task WhenIGet(string path)
    {
        var response = await Client.GetAsync(path);
        _scenarioContext[nameof(HttpResponseMessage)] = response;
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        var response = _scenarioContext.Get<HttpResponseMessage>(nameof(HttpResponseMessage));
        Assert.Equal(statusCode, (int)response.StatusCode);
    }
}
