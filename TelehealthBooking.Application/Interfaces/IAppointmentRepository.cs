using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Application.Interfaces;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime scheduledTimeUtc, CancellationToken cancellationToken);
}