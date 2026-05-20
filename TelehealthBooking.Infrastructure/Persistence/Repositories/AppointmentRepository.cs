using Microsoft.EntityFrameworkCore;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Appointments.FindAsync([id], cancellationToken);
    }

    public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .OrderBy(a => a.ScheduledTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.ScheduledTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.ScheduledTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime scheduledTimeUtc, Guid? excludeAppointmentId, CancellationToken cancellationToken)
    {
        var timeWindowStart = scheduledTimeUtc.AddMinutes(-30);
        var timeWindowEnd = scheduledTimeUtc.AddMinutes(30);

        return await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctorId
                        && a.ScheduledTimeUtc >= timeWindowStart
                        && a.ScheduledTimeUtc <= timeWindowEnd
                        && a.Status != "Cancelled"
                        && a.Id != excludeAppointmentId,
                      cancellationToken);
    }
}