using FluentAssertions;
using Moq;
using TelehealthBooking.Application.Features.Doctors.Commands;
using TelehealthBooking.Application.Features.Doctors.Queries;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;
using Xunit;

namespace TelehealthBooking.Tests.ApplicationTests;

public class DoctorCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _mockRepository;
    private readonly CreateDoctorCommandHandler _createHandler;
    private readonly UpdateDoctorCommandHandler _updateHandler;
    private readonly DeleteDoctorCommandHandler _deleteHandler;
    private readonly GetDoctorByIdQueryHandler _getByIdHandler;
    private readonly GetAllDoctorsQueryHandler _getAllHandler;

    public DoctorCommandHandlerTests()
    {
        _mockRepository = new Mock<IDoctorRepository>();
        _createHandler = new CreateDoctorCommandHandler(_mockRepository.Object);
        _updateHandler = new UpdateDoctorCommandHandler(_mockRepository.Object);
        _deleteHandler = new DeleteDoctorCommandHandler(_mockRepository.Object);
        _getByIdHandler = new GetDoctorByIdQueryHandler(_mockRepository.Object);
        _getAllHandler = new GetAllDoctorsQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateDoctor_ShouldReturnNewId()
    {
        var command = new CreateDoctorCommand("Dr. Smith", "Cardiology");

        var result = await _createHandler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDoctorById_WhenExists_ShouldReturnDto()
    {
        var doctorId = Guid.NewGuid();
        var doctor = Doctor.Create("Dr. Smith", "Cardiology");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(doctor, doctorId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var result = await _getByIdHandler.Handle(new GetDoctorByIdQuery(doctorId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Dr. Smith");
        result.Specialization.Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetDoctorById_WhenNotFound_ShouldReturnNull()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        var result = await _getByIdHandler.Handle(new GetDoctorByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllDoctors_ShouldReturnList()
    {
        var doctors = new List<Doctor>
        {
            Doctor.Create("Dr. Smith", "Cardiology"),
            Doctor.Create("Dr. Jones", "Dermatology")
        };

        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctors);

        var result = await _getAllHandler.Handle(new GetAllDoctorsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateDoctor_WhenExists_ShouldUpdate()
    {
        var doctorId = Guid.NewGuid();
        var doctor = Doctor.Create("Dr. Smith", "Cardiology");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(doctor, doctorId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        await _updateHandler.Handle(new UpdateDoctorCommand(doctorId, "Dr. Smith Updated", "Neurology"), CancellationToken.None);

        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDoctor_WhenNotFound_ShouldThrow()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        Func<Task> act = async () => await _updateHandler.Handle(
            new UpdateDoctorCommand(Guid.NewGuid(), "Name", "Spec"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor not found.");
    }

    [Fact]
    public async Task DeleteDoctor_WhenExists_ShouldDelete()
    {
        var doctorId = Guid.NewGuid();
        var doctor = Doctor.Create("Dr. Smith", "Cardiology");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(doctor, doctorId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        await _deleteHandler.Handle(new DeleteDoctorCommand(doctorId), CancellationToken.None);

        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDoctor_WhenNotFound_ShouldThrow()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        Func<Task> act = async () => await _deleteHandler.Handle(
            new DeleteDoctorCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor not found.");
    }
}
