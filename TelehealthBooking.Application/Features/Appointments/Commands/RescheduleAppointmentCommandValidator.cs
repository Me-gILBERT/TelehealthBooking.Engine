using FluentValidation;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.NewScheduledTimeUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("You cannot reschedule an appointment to the past.");
    }
}
