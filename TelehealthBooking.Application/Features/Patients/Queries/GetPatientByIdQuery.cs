using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Patients.Queries;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto?>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto?>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDto?> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null) return null;

        return MapToDto(patient);
    }

    private static PatientDto MapToDto(Patient p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Email = p.Email,
        CreatedAtUtc = p.CreatedAtUtc,
        UpdatedAtUtc = p.UpdatedAtUtc
    };
}
