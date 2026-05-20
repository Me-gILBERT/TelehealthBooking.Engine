using FluentValidation;

namespace TelehealthBooking.Application.Features.Patients.Commands;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(200).EmailAddress();
    }
}
