using DjTracksSessions.Api;
using FluentResults;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();

public partial class Program;
