using System.Net;
using System.Net.Http.Json;
using AccountingSystem.Api.Controllers;

namespace TestProject;

public sealed class CommonControllerFugaLiveHttpTests
{
    private static bool TryGetLiveBaseUri(out Uri? uri)
    {
        var raw = Environment.GetEnvironmentVariable("LIVE_API_BASE_URL");

        if (string.IsNullOrWhiteSpace(raw))
        {
            uri = null;
            return false;
        }

        // Allow either with or without trailing slash.
        if (!raw.EndsWith('/'))
        {
            raw += "/";
        }

        if (!Uri.TryCreate(raw, UriKind.Absolute, out uri))
        {
            uri = null;
            return false;
        }

        return true;
    }

    private static bool TryCreateLiveClient(out HttpClient? client)
    {
        if (!TryGetLiveBaseUri(out var baseUri))
        {
            client = null;
            return false;
        }

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
        };

        client = new HttpClient(handler)
        {
            BaseAddress = baseUri,
        };

        return true;
    }

    [Fact]
    public async Task Live_PostFuga_ReturnsBadRequest_WhenIdIsNonPositive()
    {
        if (!TryCreateLiveClient(out var client))
        {
            // LIVE_API_BASE_URL が無い場合は「実サーバ疎通テスト」を実行しない
            // (xUnit v2.5.3 の SkipException は public ctor を持たないため、実行時スキップ表現が難しい)
            return;
        }

        var liveClient = client!;
        using var _ = liveClient;

        var response = await liveClient.PostAsJsonAsync("common/fuga", new { id = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Id must be positive", body);
    }

    [Fact]
    public async Task Live_PostFuga_ReturnsOk_WithExpectedPayload_WhenIdIsOddPositiveAndNotSpecial()
    {
        if (!TryCreateLiveClient(out var client))
        {
            return;
        }

        var liveClient = client!;
        using var _ = liveClient;

        var response = await liveClient.PostAsJsonAsync("common/fuga", new CommonController.FugaRequest(1));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CommonController.FugaResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Hana", payload.Name);
        Assert.Equal(30, payload.Age);
    }
}
