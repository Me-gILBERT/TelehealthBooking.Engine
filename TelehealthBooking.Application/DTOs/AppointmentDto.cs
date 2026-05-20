namespace TelehealthBooking.Application.DTOs;

public class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledTimeUtc { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CancellationReason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
