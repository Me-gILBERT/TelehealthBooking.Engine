using FluentValidation;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.ScheduledTimeUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("You cannot book an appointment in the past.");
    }
}