using FluentValidation;

namespace TelehealthBooking.Application.Features.Doctors.Commands;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(200);
    }
}
