namespace TelehealthBooking.Application.DTOs;

public class DoctorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Specialization { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
