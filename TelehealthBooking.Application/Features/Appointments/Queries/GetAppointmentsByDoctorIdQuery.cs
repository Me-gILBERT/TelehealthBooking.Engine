using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Appointments.Queries;

public record GetAppointmentsByDoctorIdQuery(Guid DoctorId) : IRequest<List<AppointmentDto>>;

public class GetAppointmentsByDoctorIdQueryHandler : IRequestHandler<GetAppointmentsByDoctorIdQuery, List<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByDoctorIdQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsByDoctorIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(request.DoctorId, cancellationToken);
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
