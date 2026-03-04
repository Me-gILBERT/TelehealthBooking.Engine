using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence.Repositories;

// We implement the interface from the Application layer
public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    // Inject the DbContext
    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime scheduledTimeUtc, CancellationToken cancellationToken)
    {
        // Business rule translated to SQL: Is there an active appointment within 30 minutes of this time?
        var timeWindowStart = scheduledTimeUtc.AddMinutes(-30);
        var timeWindowEnd = scheduledTimeUtc.AddMinutes(30);

        return await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctorId
                        && a.ScheduledTimeUtc >= timeWindowStart
                        && a.ScheduledTimeUtc <= timeWindowEnd
                        && a.Status != "Cancelled",
                      cancellationToken);
    }
}