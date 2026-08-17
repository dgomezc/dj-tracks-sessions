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

    [Fact]
    public async Task Database_connection_diagnostic_returns_connected_state_when_probe_is_healthy()
    {
        var response = await _client.GetAsync("/configuration/database-connection");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("connected", document.RootElement.GetProperty("state").GetString());
        Assert.DoesNotContain("Healthy for test.", json);
    }

    [Fact]
    public async Task Database_connection_diagnostic_returns_unavailable_state_when_probe_is_unhealthy()
    {
        var response = await new UnhealthyApiFactory().CreateClient().GetAsync("/configuration/database-connection");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("unavailable", document.RootElement.GetProperty("state").GetString());
        Assert.DoesNotContain("Unhealthy for test.", json);
    }

    [Fact]
    public async Task Database_connection_diagnostic_hides_probe_exceptions()
    {
        var response = await new ThrowingApiFactory().CreateClient().GetAsync("/configuration/database-connection");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("test exception", json);
    }

    [Fact]
    public async Task OpenApi_document_returns_json_with_api_metadata()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("json", response.Content.Headers.ContentType?.MediaType ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("DJ Tracks & Sessions API", document.RootElement.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("v1", document.RootElement.GetProperty("info").GetProperty("version").GetString());
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/examples/{id}", out _));
    }

    [Fact]
    public async Task Scalar_reference_returns_html()
    {
        var response = await _client.GetAsync("/scalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("html", response.Content.Headers.ContentType?.MediaType ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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

    private sealed class ThrowingDatabaseHealthProbe : IDatabaseHealthProbe
    {
        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("test exception");
        }
    }

    private sealed class ThrowingApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDatabaseHealthProbe>();
                services.AddScoped<IDatabaseHealthProbe, ThrowingDatabaseHealthProbe>();
            });
        }
    }
}
