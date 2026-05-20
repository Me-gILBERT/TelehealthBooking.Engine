using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Appointments.Queries;

public record GetAllAppointmentsQuery : IRequest<List<AppointmentDto>>;

public class GetAllAppointmentsQueryHandler : IRequestHandler<GetAllAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAllAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<AppointmentDto>> Handle(GetAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return appointments.Select(MapToDto).ToList();
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        DoctorId = a.DoctorId,
        ScheduledTimeUtc = a.ScheduledTimeUtc,
        Status = a.Status,
        CancellationReason = a.CancellationReason,
        CreatedAtUtc = a.CreatedAtUtc,
        UpdatedAtUtc = a.UpdatedAtUtc
    };
}
