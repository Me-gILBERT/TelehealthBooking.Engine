using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        // Defensive Rules
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();

        // Ensure the appointment is in the future
        RuleFor(x => x.ScheduledTimeUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("You cannot book an appointment in the past.");
    }
}