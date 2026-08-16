using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DjTracksSessions.IntegrationTests;

public sealed class ApiProblemDetailsTests : IClassFixture<ApiProblemDetailsTests.ApiFactory>
{
    private readonly HttpClient _client;

    public ApiProblemDetailsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Example_endpoint_returns_success_payload()
    {
        var response = await _client.GetAsync("/examples/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ExampleResponse>();

        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Id);
        Assert.Equal("example", payload.Name);
    }

    [Fact]
    public async Task Example_endpoint_maps_expected_failure_to_problem_details()
    {
        var response = await _client.GetAsync("/examples/2");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var code = document.RootElement.GetProperty("errorCode").GetString();

        Assert.NotNull(problem);
        Assert.Equal("Example not found.", problem!.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal(DjTracksSessions.Contracts.ApiErrorCodes.ExampleNotFound, code);
    }

    private sealed record ExampleResponse(int Id, string Name);

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
    }
}
