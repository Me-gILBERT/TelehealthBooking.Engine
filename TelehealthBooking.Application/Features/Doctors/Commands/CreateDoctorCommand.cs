using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Doctors.Commands;

public record CreateDoctorCommand(string Name, string Specialization) : IRequest<Guid>;

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Guid>
{
    private readonly IDoctorRepository _doctorRepository;

    public CreateDoctorCommandHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = Doctor.Create(request.Name, request.Specialization);
        await _doctorRepository.AddAsync(doctor, cancellationToken);
        return doctor.Id;
    }
}
