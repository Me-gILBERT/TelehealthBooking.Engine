using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Patients.Commands;

public record UpdatePatientCommand(Guid Id, string Name, string Email) : IRequest<Unit>;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;

    public UpdatePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Unit> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Exception("Patient not found.");

        patient.Update(request.Name, request.Email);
        await _patientRepository.UpdateAsync(patient, cancellationToken);

        return Unit.Value;
    }
}
