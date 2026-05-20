using MediatR;
using TelehealthBooking.Application.Interfaces;

namespace TelehealthBooking.Application.Features.Appointments.Commands;

public record CancelAppointmentCommand(
    Guid AppointmentId,
    string Reason) : IRequest<Unit>;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Unit> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new Exception("Appointment not found.");

        appointment.Cancel(request.Reason);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return Unit.Value;
    }
}
