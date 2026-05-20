using FluentAssertions;
using Moq;
using TelehealthBooking.Application.Features.Patients.Commands;
using TelehealthBooking.Application.Features.Patients.Queries;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Domain.Entities;
using Xunit;

namespace TelehealthBooking.Tests.ApplicationTests;

public class PatientCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _mockRepository;
    private readonly CreatePatientCommandHandler _createHandler;
    private readonly UpdatePatientCommandHandler _updateHandler;
    private readonly DeletePatientCommandHandler _deleteHandler;
    private readonly GetPatientByIdQueryHandler _getByIdHandler;
    private readonly GetAllPatientsQueryHandler _getAllHandler;

    public PatientCommandHandlerTests()
    {
        _mockRepository = new Mock<IPatientRepository>();
        _createHandler = new CreatePatientCommandHandler(_mockRepository.Object);
        _updateHandler = new UpdatePatientCommandHandler(_mockRepository.Object);
        _deleteHandler = new DeletePatientCommandHandler(_mockRepository.Object);
        _getByIdHandler = new GetPatientByIdQueryHandler(_mockRepository.Object);
        _getAllHandler = new GetAllPatientsQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task CreatePatient_ShouldReturnNewId()
    {
        var command = new CreatePatientCommand("John Doe", "john@example.com");

        var result = await _createHandler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPatientById_WhenExists_ShouldReturnDto()
    {
        var patientId = Guid.NewGuid();
        var patient = Patient.Create("John Doe", "john@example.com");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(patient, patientId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var result = await _getByIdHandler.Handle(new GetPatientByIdQuery(patientId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetPatientById_WhenNotFound_ShouldReturnNull()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var result = await _getByIdHandler.Handle(new GetPatientByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllPatients_ShouldReturnList()
    {
        var patients = new List<Patient>
        {
            Patient.Create("John Doe", "john@example.com"),
            Patient.Create("Jane Doe", "jane@example.com")
        };

        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        var result = await _getAllHandler.Handle(new GetAllPatientsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdatePatient_WhenExists_ShouldUpdate()
    {
        var patientId = Guid.NewGuid();
        var patient = Patient.Create("John Doe", "john@example.com");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(patient, patientId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        await _updateHandler.Handle(new UpdatePatientCommand(patientId, "John Updated", "john.updated@example.com"), CancellationToken.None);

        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePatient_WhenNotFound_ShouldThrow()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        Func<Task> act = async () => await _updateHandler.Handle(
            new UpdatePatientCommand(Guid.NewGuid(), "Name", "email@test.com"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Patient not found.");
    }

    [Fact]
    public async Task DeletePatient_WhenExists_ShouldDelete()
    {
        var patientId = Guid.NewGuid();
        var patient = Patient.Create("John Doe", "john@example.com");
        var idField = typeof(BaseEntity<Guid>).GetProperty("Id");
        idField?.SetValue(patient, patientId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        await _deleteHandler.Handle(new DeletePatientCommand(patientId), CancellationToken.None);

        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePatient_WhenNotFound_ShouldThrow()
    {
        _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        Func<Task> act = async () => await _deleteHandler.Handle(
            new DeletePatientCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Patient not found.");
    }
}
