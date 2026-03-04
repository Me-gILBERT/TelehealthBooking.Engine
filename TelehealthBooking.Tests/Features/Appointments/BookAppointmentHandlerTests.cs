using Moq;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;
using Xunit;

namespace TelehealthBooking.Tests.Features.Appointments;

public class BookAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repositoryMock;
    private readonly BookAppointmentCommandHandler _handler;

    public BookAppointmentCommandHandlerTests()
    {
        // Setup the fake repository
        _repositoryMock = new Mock<IAppointmentRepository>();

        // Inject the fake repository into our handler
        _handler = new BookAppointmentCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallAddAsync()
    {
        // Arrange
        var command = new BookAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1)
        );

        // Tell the mock database to return 'false' (no overlap) when asked
        _repositoryMock.Setup(repo => repo.HasOverlappingAppointmentAsync(
            command.DoctorId, command.ScheduledTimeUtc, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        // Verify the repository was called exactly once to save the appointment
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}