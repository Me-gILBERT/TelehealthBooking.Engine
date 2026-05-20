using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelehealthBooking.Application.Features.Patients.Commands;
using TelehealthBooking.Application.Features.Patients.Queries;

namespace TelehealthBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientCommand command)
    {
        var patientId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPatientById), new { id = patientId }, patientId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPatientById(Guid id)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPatients()
    {
        var result = await _mediator.Send(new GetAllPatientsQuery());
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Error = "Id mismatch" });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        await _mediator.Send(new DeletePatientCommand(id));
        return NoContent();
    }
}
