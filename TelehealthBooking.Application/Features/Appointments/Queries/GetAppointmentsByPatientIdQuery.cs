using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Appointments.Queries;

public record GetAppointmentsByPatientIdQuery(Guid PatientId) : IRequest<List<AppointmentDto>>;

public class GetAppointmentsByPatientIdQueryHandler : IRequestHandler<GetAppointmentsByPatientIdQuery, List<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsByPatientIdQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsByPatientIdQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);
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
