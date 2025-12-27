using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountingSystem.Api.Tests;

public sealed class WeatherForecastAuthTests : IClassFixture<PostgresApiFactory>
{
    private readonly PostgresApiFactory _factory;

    public WeatherForecastAuthTests(PostgresApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeatherForecast_FailsBeforeLogin_SucceedsAfterLogin()
    {
        using var client = _factory.Factory.CreateClient();

        // Before login
        using (var before = await client.GetAsync("/WeatherForecast"))
        {
            Assert.Equal(HttpStatusCode.Unauthorized, before.StatusCode);
        }

        // Register (creates company + user)
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd1";
        var companyName = $"Company {Guid.NewGuid():N}";

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password,
            companyName,
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerJson = await registerResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<TokenResponseDto>(registerJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize token response.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // After login
        using (var after = await client.GetAsync("/WeatherForecast"))
        {
            Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record TokenResponseDto(string AccessToken, DateTimeOffset ExpiresAtUtc);
}
