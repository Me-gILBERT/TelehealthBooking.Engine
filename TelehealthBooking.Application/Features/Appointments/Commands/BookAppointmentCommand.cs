using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

// 1. The Command (The Data)
public record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledTimeUtc) : IRequest<Guid>;

// 2. The Handler (The Logic)
public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
{
    private readonly IAppointmentRepository _appointmentRepository;

    // Injecting the interface (Inversion of Control)
    public BookAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Check for overlaps (Business Rule)
        bool isOverlap = await _appointmentRepository.HasOverlappingAppointmentAsync(
            request.DoctorId,
            request.ScheduledTimeUtc,
            cancellationToken);

        if (isOverlap)
        {
            // In an enterprise app, we'd throw a custom domain exception here
            throw new Exception("Doctor is already booked for this time slot.");
        }

        // Create the entity
        var appointment = Appointment.Create(
            request.PatientId,
            request.DoctorId,
            request.ScheduledTimeUtc);

        // Save it via the interface
        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return appointment.Id;
    }
}