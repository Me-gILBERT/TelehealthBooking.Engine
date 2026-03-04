using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;
using Xunit;

namespace TelehealthBooking.Tests.ApplicationTests;

public class BookAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _mockRepository;
    private readonly BookAppointmentCommandHandler _handler;

    public BookAppointmentCommandHandlerTests()
    {
        // 1. ARRANGE (Global Setup)
        // We create a "Mock" of the interface. It acts like a database, but runs entirely in memory.
        _mockRepository = new Mock<IAppointmentRepository>();

        // We inject the mock into the handler, achieving true isolated testing.
        _handler = new BookAppointmentCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenNoOverlap_ShouldReturnNewAppointmentId()
    {
        // Arrange
        var command = new BookAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        // Tell the mock database to return 'false' (no overlap) when asked
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            command.DoctorId, command.ScheduledTimeUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty(); // The Guid should not be 00000000-0000-0000-0000-000000000000

        // Verify that the repository's AddAsync method was called exactly one time
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOverlapExists_ShouldThrowException()
    {
        // Arrange
        var command = new BookAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        // Negative Testing: Tell the mock database to return 'true' (overlap exists!)
        _mockRepository.Setup(repo => repo.HasOverlappingAppointmentAsync(
            command.DoctorId, command.ScheduledTimeUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        // We don't await it immediately, we store the action so we can catch the exception
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor is already booked for this time slot.");

        // Verify that the database NEVER tried to save this bad data
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}