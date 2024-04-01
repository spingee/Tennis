namespace Tennis.App.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

public class Fixture : IDisposable
{
    public HttpClient HttpClient { get; }
    public WebApplicationFactory<Program> WebApplication { get; init; }

    public Fixture()
    {
        Environment.SetEnvironmentVariable("Orleans:InMemory", "true", EnvironmentVariableTarget.Process);
        WebApplication = new WebApplicationFactory<Program>().WithWebHostBuilder(
            builder =>
            {

            });
        HttpClient = WebApplication.CreateClient();
    }

    public void Dispose()
    {
        WebApplication.Dispose();
    }
}