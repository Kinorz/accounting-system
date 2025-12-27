using System.Net;
using Xunit.Abstractions;

namespace AccountingSystem.Api.Tests;

public class WeatherForecastApiTests : IClassFixture<PostgresApiFactory>
{
    private readonly PostgresApiFactory _factory;
    private readonly ITestOutputHelper _output;

    public WeatherForecastApiTests(PostgresApiFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        using var client = _factory.Factory.CreateClient();

        using var response = await client.GetAsync("/WeatherForecast");
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"StatusCode: {(int)response.StatusCode} {response.StatusCode}");
        _output.WriteLine("Headers:");
        foreach (var h in response.Headers)
            _output.WriteLine($"  {h.Key}: {string.Join(", ", h.Value)}");

        _output.WriteLine("Body:");
        _output.WriteLine(body);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}