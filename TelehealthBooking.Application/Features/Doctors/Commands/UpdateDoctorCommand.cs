using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Doctors.Commands;

public record UpdateDoctorCommand(Guid Id, string Name, string Specialization) : IRequest<Unit>;

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, Unit>
{
    private readonly IDoctorRepository _doctorRepository;

    public UpdateDoctorCommandHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Unit> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Exception("Doctor not found.");

        doctor.Update(request.Name, request.Specialization);
        await _doctorRepository.UpdateAsync(doctor, cancellationToken);

        return Unit.Value;
    }
}
