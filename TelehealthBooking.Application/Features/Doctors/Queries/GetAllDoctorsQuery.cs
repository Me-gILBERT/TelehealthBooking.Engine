using MediatR;
using TelehealthBooking.Application.DTOs;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Doctors.Queries;

public record GetAllDoctorsQuery : IRequest<List<DoctorDto>>;

public class GetAllDoctorsQueryHandler : IRequestHandler<GetAllDoctorsQuery, List<DoctorDto>>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetAllDoctorsQueryHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<List<DoctorDto>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _doctorRepository.GetAllAsync(cancellationToken);
        return doctors.Select(MapToDto).ToList();
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
