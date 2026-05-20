using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Doctor doctor, CancellationToken cancellationToken);
    Task UpdateAsync(Doctor doctor, CancellationToken cancellationToken);
    Task DeleteAsync(Doctor doctor, CancellationToken cancellationToken);
}
