using DjTracksSessions.Api;
using DjTracksSessions.Api.Validation;
using DjTracksSessions.Contracts;
using FluentResults;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<ValidationExampleRequestValidator>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

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
