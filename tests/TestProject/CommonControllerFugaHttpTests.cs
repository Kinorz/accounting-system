using System.Net;
using System.Net.Http.Json;
using AccountingSystem.Api.Controllers;

namespace TestProject;

public sealed class CommonControllerFugaHttpTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public CommonControllerFugaHttpTests(TestApiFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task PostFuga_ReturnsBadRequest_WhenIdIsNonPositive()
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Id must be positive", body);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PostFuga_ReturnsBadRequest_WhenIdIsNonPositive_Theory(int id)
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Id must be positive", body);
    }

    [Fact]
    public async Task PostFuga_ReturnsNotFound_WhenIdIsEvenPositive()
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id = 2 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task PostFuga_ReturnsNotFound_WhenIdIsEvenPositive_Theory(int id)
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostFuga_ReturnsTeapot_WhenIdIs42()
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id = 42 });

        Assert.Equal(418, (int)response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("teapot", body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(FugaTestCases.SpecialStatusCodeCases), MemberType = typeof(FugaTestCases))]
    public async Task PostFuga_ReturnsExpectedStatusCode_ForSpecialIds_Theory(int id, int expectedStatusCode, string expectedMessage)
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new { id });

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);

        // StatusCode(x, "message") のボディは文字列になるので、部分一致で確認する。
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedMessage, body);
    }

    [Fact]
    public async Task PostFuga_ReturnsOk_WithExpectedPayload_WhenIdIsOddPositiveAndNotSpecial()
    {
        var response = await _client.PostAsJsonAsync("/common/fuga", new CommonController.FugaRequest(1));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CommonController.FugaResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Hana", payload.Name);
        Assert.Equal(30, payload.Age);
    }
}
