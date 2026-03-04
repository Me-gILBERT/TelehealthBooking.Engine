using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelehealthBooking.Domain.Entities;

// Inherit from BaseEntity and specify that the ID will be a Guid
public sealed class Appointment : BaseEntity<Guid>
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime ScheduledTimeUtc { get; private set; }
    public string Status { get; private set; } = string.Empty; // e.g., "Pending", "Confirmed", "Cancelled"
    public string? CancellationReason { get; private set; }

    // Private constructor required by Entity Framework
    private Appointment() { }

    // Public factory method to create a new appointment safely
    public static Appointment Create(Guid patientId, Guid doctorId, DateTime scheduledTimeUtc)
    {
        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledTimeUtc = scheduledTimeUtc,
            Status = "Pending"
        };
    }

    // Business Logic: Encapsulating the cancellation process
    public void Cancel(string reason)
    {
        Status = "Cancelled";
        CancellationReason = reason;
        MarkAsUpdated();
    }

    public void Confirm()
    {
        Status = "Confirmed";
        MarkAsUpdated();
    }
}