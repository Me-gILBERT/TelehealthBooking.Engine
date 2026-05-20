using Microsoft.EntityFrameworkCore;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Doctors.FindAsync([id], cancellationToken);
    }

    public async Task<List<Doctor>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Doctors.OrderBy(d => d.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        await _context.Doctors.AddAsync(doctor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
