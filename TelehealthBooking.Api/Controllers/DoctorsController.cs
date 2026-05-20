using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelehealthBooking.Application.Features.Doctors.Commands;
using TelehealthBooking.Application.Features.Doctors.Queries;

namespace TelehealthBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorCommand command)
    {
        var doctorId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDoctorById), new { id = doctorId }, doctorId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDoctorById(Guid id)
    {
        var result = await _mediator.Send(new GetDoctorByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDoctors()
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery());
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Error = "Id mismatch" });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        await _mediator.Send(new DeleteDoctorCommand(id));
        return NoContent();
    }
}
