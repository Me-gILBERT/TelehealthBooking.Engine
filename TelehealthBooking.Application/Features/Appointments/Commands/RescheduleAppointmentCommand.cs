using MediatR;
using TelehealthBooking.Application.Interfaces;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTime NewScheduledTimeUtc) : IRequest<Unit>;

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public RescheduleAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Unit> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new Exception("Appointment not found.");

        if (appointment.Status == "Cancelled")
            throw new Exception("Cannot reschedule a cancelled appointment.");

        bool isOverlap = await _appointmentRepository.HasOverlappingAppointmentAsync(
            appointment.DoctorId,
            request.NewScheduledTimeUtc,
            appointment.Id,
            cancellationToken);

        if (isOverlap)
            throw new Exception("Doctor is already booked for this time slot.");

        appointment.Reschedule(request.NewScheduledTimeUtc);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return Unit.Value;
    }
}
