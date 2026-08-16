using FluentValidation;
using FluentValidation.Results;
using FluentResults;
using DjTracksSessions.Contracts;

namespace DjTracksSessions.Api.Validation;

public static class ApiValidation
{
    public static async Task<Result<T>> ValidateAsync<T>(
        T request,
        IValidator<T> validator,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return Result.Ok(request);
        }

        var error = new Error("Request validation failed.")
            .WithMetadata("StatusCode", StatusCodes.Status400BadRequest)
            .WithMetadata("errorCode", ApiErrorCodes.ValidationFailed)
            .WithMetadata("validationErrors", validationResult.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray()));

        return Result.Fail<T>(error);
    }
}
