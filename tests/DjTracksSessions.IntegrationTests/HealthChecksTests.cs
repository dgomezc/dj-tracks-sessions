using System.Net;
using System.Text.Json;
using DjTrackSessions.Infrastructure.Health;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class HealthChecksTests
{
    private readonly HttpClient _client;

    public HealthChecksTests()
    {
        _client = new HealthyApiFactory().CreateClient();
    }

    [Fact]
    public async Task Health_endpoint_returns_database_status_payload_when_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("Healthy", document.RootElement.GetProperty("checks").GetProperty("database").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Health_endpoint_returns_unhealthy_status_when_database_probe_fails()
    {
        var response = await new UnhealthyApiFactory().CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Unhealthy", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("Unhealthy", document.RootElement.GetProperty("checks").GetProperty("database").GetProperty("status").GetString());
    }

    private sealed class HealthyDatabaseHealthProbe : IDatabaseHealthProbe
    {
        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Healthy for test."));
        }
    }

    private sealed class UnhealthyDatabaseHealthProbe : IDatabaseHealthProbe
    {
        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Unhealthy for test."));
        }
    }

    private sealed class HealthyApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDatabaseHealthProbe>();
                services.AddScoped<IDatabaseHealthProbe, HealthyDatabaseHealthProbe>();
            });
        }
    }

    private sealed class UnhealthyApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDatabaseHealthProbe>();
                services.AddScoped<IDatabaseHealthProbe, UnhealthyDatabaseHealthProbe>();
            });
        }
    }
}
