using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Patients.Commands;

public record DeletePatientCommand(Guid Id) : IRequest<Unit>;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;

    public DeletePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Unit> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Exception("Patient not found.");

        await _patientRepository.DeleteAsync(patient, cancellationToken);
        return Unit.Value;
    }
}
