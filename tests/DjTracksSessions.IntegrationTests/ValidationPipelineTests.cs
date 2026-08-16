using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class ValidationPipelineTests : IClassFixture<ValidationPipelineTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ValidationPipelineTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Validation_example_endpoint_returns_created_for_valid_input()
    {
        var response = await _client.PostAsJsonAsync("/validation-examples", new { name = "valid" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ValidationExampleResponse>();

        Assert.NotNull(payload);
        Assert.Equal("valid", payload!.Name);
    }

    [Fact]
    public async Task Validation_example_endpoint_maps_invalid_input_to_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/validation-examples", new { name = string.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Request validation failed.", document.RootElement.GetProperty("title").GetString());
        Assert.Equal(DjTracksSessions.Contracts.ApiErrorCodes.ValidationFailed, document.RootElement.GetProperty("errorCode").GetString());
        Assert.True(document.RootElement.TryGetProperty("validationErrors", out var validationErrors));
        Assert.True(validationErrors.TryGetProperty("Name", out var nameErrors));
        Assert.NotEmpty(nameErrors.EnumerateArray());
    }

    private sealed record ValidationExampleResponse(string Name);

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
    }
}
