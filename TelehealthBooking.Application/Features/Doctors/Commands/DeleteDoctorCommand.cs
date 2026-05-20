using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Doctors.Commands;

public record DeleteDoctorCommand(Guid Id) : IRequest<Unit>;

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, Unit>
{
    private readonly IDoctorRepository _doctorRepository;

    public DeleteDoctorCommandHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Unit> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Exception("Doctor not found.");

        await _doctorRepository.DeleteAsync(doctor, cancellationToken);
        return Unit.Value;
    }
}
