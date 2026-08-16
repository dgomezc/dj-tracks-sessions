using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DjTracksSessions.Api;

public static class ApiProblemDetails
{
    private const string ErrorCodeKey = "errorCode";

    public static IResult FromResult<T>(Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return FromError(result.Errors.First());
    }

    public static IResult FromError(IError error)
    {
        var statusCode = error.Metadata.TryGetValue("StatusCode", out var statusValue) && statusValue is int status
            ? status
            : StatusCodes.Status400BadRequest;

        var extensions = new Dictionary<string, object?>
        {
            [ErrorCodeKey] = GetErrorCode(error)
        };

        foreach (var metadata in error.Metadata)
        {
            if (metadata.Key is "StatusCode" or ErrorCodeKey)
            {
                continue;
            }

            extensions[metadata.Key] = metadata.Value;
        }

        var problem = Results.Problem(
            title: error.Message,
            statusCode: statusCode,
            extensions: extensions);

        return problem;
    }

    public static Error NotFound(string message, string errorCode)
    {
        return new Error(message)
            .WithMetadata("StatusCode", StatusCodes.Status404NotFound)
            .WithMetadata(ErrorCodeKey, errorCode);
    }

    private static string GetErrorCode(IError error)
    {
        return error.Metadata.TryGetValue(ErrorCodeKey, out var code) && code is string value
            ? value
            : "unknown_error";
    }
}
