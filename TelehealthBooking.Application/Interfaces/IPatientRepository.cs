using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken);
    Task DeleteAsync(Patient patient, CancellationToken cancellationToken);
}
