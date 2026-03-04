using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelehealthBooking.Application.Features.Appointments.Commands;

namespace TelehealthBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    // Inject MediatR instead of the database or repository
    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentCommand command)
    {
        try
        {
            // Send the command into the Application Layer
            var appointmentId = await _mediator.Send(command);

            // Return a 201 Created with the new ID
            return CreatedAtAction(nameof(BookAppointment), new { id = appointmentId }, appointmentId);
        }
        catch (Exception ex)
        {
            // If our domain validation fails, it throws an exception which we catch here.
            // In a fully polished app, we'd use a Global Exception Handling Middleware instead of a try/catch.
            return BadRequest(new { Error = ex.Message });
        }
    }
}