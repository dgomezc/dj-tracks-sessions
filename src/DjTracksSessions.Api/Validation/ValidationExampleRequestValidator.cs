using DjTracksSessions.Contracts;
using FluentValidation;

namespace DjTracksSessions.Api.Validation;

public sealed class ValidationExampleRequestValidator : AbstractValidator<ValidationExampleRequest>
{
    public ValidationExampleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty();
    }
}
