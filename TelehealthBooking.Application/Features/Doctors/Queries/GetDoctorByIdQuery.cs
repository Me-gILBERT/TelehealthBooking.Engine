using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Doctors.Queries;

public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto?>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, DoctorDto?>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorByIdQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<DoctorDto?> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor is null) return null;

        return MapToDto(doctor);
    }

    private static DoctorDto MapToDto(Doctor d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Specialization = d.Specialization,
        CreatedAtUtc = d.CreatedAtUtc,
        UpdatedAtUtc = d.UpdatedAtUtc
    };
}
