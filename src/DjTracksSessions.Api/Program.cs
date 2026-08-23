using DjTracksSessions.Api;
using DjTracksSessions.Api.Validation;
using DjTracksSessions.Api.Features.Configuration.DatabaseConnection;
using DjTracksSessions.Api.Features.Library.Metadata;
using DjTracksSessions.Api.Features.Library.Pending;
using DjTracksSessions.Api.Features.Library.Browse;
using DjTracksSessions.Api.Features.Playback;
using DjTracksSessions.Api.Features.Sessions.Browse;
using DjTracksSessions.Api.Features.Sessions.Metadata;
using DjTracksSessions.Contracts;
using DjTrackSessions.Infrastructure;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "DJ Tracks & Sessions API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});
builder.Services.AddValidatorsFromAssemblyContaining<ValidationExampleRequestValidator>();
builder.Services.AddInfrastructurePersistence(connectionString);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("DJ Tracks & Sessions API"));

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
app.MapTrackMetadata();
app.MapPendingMove();
app.MapFilesystemBrowse();
app.MapAudioStreaming();
app.MapSessionBrowse();
app.MapSessionMetadata();

app.MapGet("/examples/{id}", (int id) =>
{
    if (id == 1)
    {
        return ApiProblemDetails.FromResult(Result.Ok(new { id, name = "example" }), value => Results.Ok(value));
    }

    return ApiProblemDetails.FromResult(Result.Fail<object>(ApiProblemDetails.NotFound("Example not found.", DjTracksSessions.Contracts.ApiErrorCodes.ExampleNotFound)), value => Results.Ok(value));
})
    .WithName("GetExample")
    .WithSummary("Get an example by ID")
    .WithDescription("Returns the sample payload for ID 1 or a problem details response for other IDs.");

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
})
    .WithName("CreateValidationExample")
    .WithSummary("Create a validation example")
    .WithDescription("Validates the request and returns the created example name.");

app.Run();

public partial class Program;
