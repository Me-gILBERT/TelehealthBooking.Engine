using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken);
    Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken);
    Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken);
    Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime scheduledTimeUtc, Guid? excludeAppointmentId, CancellationToken cancellationToken);
}