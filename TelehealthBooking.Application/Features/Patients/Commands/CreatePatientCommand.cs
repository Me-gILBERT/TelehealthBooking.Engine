using MediatR;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Features.Patients.Commands;

public record CreatePatientCommand(string Name, string Email) : IRequest<Guid>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IPatientRepository _patientRepository;

    public CreatePatientCommandHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = Patient.Create(request.Name, request.Email);
        await _patientRepository.AddAsync(patient, cancellationToken);
        return patient.Id;
    }
}
