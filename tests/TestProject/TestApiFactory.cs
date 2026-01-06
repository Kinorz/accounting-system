using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TestProject;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string? _previousEnvironment;
    private readonly string? _previousConnectionString;

    public TestApiFactory()
    {
        // Program.cs は ConnectionStrings:Default が無いと起動時に例外になるため、
        // 最小HTTPテストでは「形だけ」接続文字列を環境変数で差し込む。
        // /common/* はDBを使わないので、実際にDBへ接続はしない。
        _previousEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Default",
            "Host=localhost;Database=dummy;Username=dummy;Password=dummy");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _previousEnvironment);
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _previousConnectionString);
    }
}
