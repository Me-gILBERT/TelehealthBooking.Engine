using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Features.Appointments.Queries;

namespace TelehealthBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentCommand command)
    {
        var appointmentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAppointmentById), new { id = appointmentId }, appointmentId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var result = await _mediator.Send(new GetAppointmentByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAppointments()
    {
        var result = await _mediator.Send(new GetAllAppointmentsQuery());
        return Ok(result);
    }

    [HttpGet("by-patient/{patientId:guid}")]
    public async Task<IActionResult> GetAppointmentsByPatient(Guid patientId)
    {
        var result = await _mediator.Send(new GetAppointmentsByPatientIdQuery(patientId));
        return Ok(result);
    }

    [HttpGet("by-doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetAppointmentsByDoctor(Guid doctorId)
    {
        var result = await _mediator.Send(new GetAppointmentsByDoctorIdQuery(doctorId));
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        await _mediator.Send(new CancelAppointmentCommand(id, request.Reason));
        return NoContent();
    }

    [HttpPut("{id:guid}/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(Guid id, [FromBody] RescheduleAppointmentRequest request)
    {
        await _mediator.Send(new RescheduleAppointmentCommand(id, request.NewScheduledTimeUtc));
        return NoContent();
    }
}

public record CancelAppointmentRequest(string Reason);
public record RescheduleAppointmentRequest(DateTime NewScheduledTimeUtc);
