using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountingSystem.Api.Tests;

public sealed class AuthApiTests : IClassFixture<PostgresApiFactory>
{
    private readonly PostgresApiFactory _factory;

    public AuthApiTests(PostgresApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Login_Me_Works()
    {
        using var client = _factory.Factory.CreateClient();

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
        var registerToken = JsonSerializer.Deserialize<TokenResponseDto>(registerJson, JsonOptions)!
            ?? throw new InvalidOperationException("Failed to deserialize register token response.");

        Assert.False(string.IsNullOrWhiteSpace(registerToken.AccessToken));

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password,
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginJson = await loginResponse.Content.ReadAsStringAsync();
        var loginToken = JsonSerializer.Deserialize<TokenResponseDto>(loginJson, JsonOptions)!
            ?? throw new InvalidOperationException("Failed to deserialize login token response.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginToken.AccessToken);

        var meResponse = await client.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meJson = await meResponse.Content.ReadAsStringAsync();
        var me = JsonSerializer.Deserialize<MeResponseDto>(meJson, JsonOptions)!
            ?? throw new InvalidOperationException("Failed to deserialize me response.");

        Assert.False(string.IsNullOrWhiteSpace(me.UserId));
        Assert.Equal(email, me.Email);
        Assert.False(string.IsNullOrWhiteSpace(me.CompanyId));
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record TokenResponseDto(string AccessToken, DateTimeOffset ExpiresAtUtc);

    private sealed record MeResponseDto(string? UserId, string? Email, string? CompanyId);
}
