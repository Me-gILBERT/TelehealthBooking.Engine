using Microsoft.EntityFrameworkCore;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Patients.FindAsync([id], cancellationToken);
    }

    public async Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Patients.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        await _context.Patients.AddAsync(patient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Patient patient, CancellationToken cancellationToken)
    {
        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
