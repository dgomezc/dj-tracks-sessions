using DjTracksSessions.Api;
using DjTracksSessions.Api.Validation;
using DjTracksSessions.Api.Features.Configuration.DatabaseConnection;
using DjTracksSessions.Contracts;
using DjTrackSessions.Infrastructure;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddValidatorsFromAssemblyContaining<ValidationExampleRequestValidator>();
builder.Services.AddInfrastructurePersistence(connectionString);

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = static async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description
                })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

app.MapDatabaseConnectionDiagnostic();

app.MapGet("/examples/{id}", (int id) =>
{
    if (id == 1)
    {
        return ApiProblemDetails.FromResult(Result.Ok(new { id, name = "example" }), value => Results.Ok(value));
    }

    return ApiProblemDetails.FromResult(Result.Fail<object>(ApiProblemDetails.NotFound("Example not found.", DjTracksSessions.Contracts.ApiErrorCodes.ExampleNotFound)), value => Results.Ok(value));
});

app.MapPost("/validation-examples", async (
    ValidationExampleRequest request,
    IValidator<ValidationExampleRequest> validator,
    CancellationToken cancellationToken) =>
{
    var validationResult = await ApiValidation.ValidateAsync(
        request,
        validator,
        cancellationToken);
    return ApiProblemDetails.FromResult(
        validationResult,
        validRequest => Results.Created($"/validation-examples/{validRequest.Name}", new ValidationExampleResponse(validRequest.Name)));
});

app.Run();

public partial class Program;
