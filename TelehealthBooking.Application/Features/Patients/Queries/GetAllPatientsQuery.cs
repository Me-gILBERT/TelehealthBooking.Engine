using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Patients.Queries;

public record GetAllPatientsQuery : IRequest<List<PatientDto>>;

public class GetAllPatientsQueryHandler : IRequestHandler<GetAllPatientsQuery, List<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetAllPatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<List<PatientDto>> Handle(GetAllPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await _patientRepository.GetAllAsync(cancellationToken);
        return patients.Select(MapToDto).ToList();
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
