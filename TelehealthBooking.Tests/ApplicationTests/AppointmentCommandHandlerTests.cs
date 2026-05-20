using FluentAssertions;
using Moq;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;
using Xunit;

namespace TelehealthBooking.Tests.ApplicationTests;

public class AppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _mockRepository;
    private readonly BookAppointmentCommandHandler _bookHandler;
    private readonly CancelAppointmentCommandHandler _cancelHandler;
    private readonly RescheduleAppointmentCommandHandler _rescheduleHandler;

    public AppointmentCommandHandlerTests()
    {
        _mockRepository = new Mock<IAppointmentRepository>();
        _bookHandler = new BookAppointmentCommandHandler(_mockRepository.Object);
        _cancelHandler = new CancelAppointmentCommandHandler(_mockRepository.Object);
        _rescheduleHandler = new RescheduleAppointmentCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task BookAppointment_WhenNoOverlap_ShouldReturnNewAppointmentId()
    {
        var command = new BookAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            command.DoctorId, command.ScheduledTimeUtc, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _bookHandler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookAppointment_WhenOverlapExists_ShouldThrowException()
    {
        var command = new BookAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            command.DoctorId, command.ScheduledTimeUtc, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> act = async () => await _bookHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor is already booked for this time slot.");
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAppointment_WhenExists_ShouldUpdateStatus()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(appointment, appointmentId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var command = new CancelAppointmentCommand(appointmentId, "Patient unavailable");

        var result = await _cancelHandler.Handle(command, CancellationToken.None);

        _mockRepository.Verify(repo => repo.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_WhenNotFound_ShouldThrowException()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var command = new CancelAppointmentCommand(Guid.NewGuid(), "Reason");

        Func<Task> act = async () => await _cancelHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment not found.");
    }

    [Fact]
    public async Task RescheduleAppointment_WhenNoOverlap_ShouldUpdateTime()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(appointment, appointmentId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            appointment.DoctorId, It.IsAny<DateTime>(), appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new RescheduleAppointmentCommand(appointmentId, DateTime.UtcNow.AddDays(2));

        var result = await _rescheduleHandler.Handle(command, CancellationToken.None);

        _mockRepository.Verify(repo => repo.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RescheduleAppointment_WhenOverlapExists_ShouldThrowException()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(appointment, appointmentId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            appointment.DoctorId, It.IsAny<DateTime>(), appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RescheduleAppointmentCommand(appointmentId, DateTime.UtcNow.AddDays(2));

        Func<Task> act = async () => await _rescheduleHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor is already booked for this time slot.");
    }

    [Fact]
    public async Task RescheduleAppointment_WhenCancelled_ShouldThrowException()
    {
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(appointment, appointmentId);
        appointment.Cancel("Already cancelled");

        _mockRepository.Setup(repo => repo.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var command = new RescheduleAppointmentCommand(appointmentId, DateTime.UtcNow.AddDays(2));

        Func<Task> act = async () => await _rescheduleHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Cannot reschedule a cancelled appointment.");
    }
}
