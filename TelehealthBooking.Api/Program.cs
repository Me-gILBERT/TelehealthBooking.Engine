using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TelehealthBooking.Api.Middleware;
using TelehealthBooking.Application.Behaviors;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Infrastructure.Persistence;
using TelehealthBooking.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<BookAppointmentCommand>());

// Add FluentValidation validators & pipeline
builder.Services.AddValidatorsFromAssemblyContaining<BookAppointmentCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-apply pending migrations on startup (idempotent — safe to run every time)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay}s...",
                attempt, maxRetries, delay.TotalSeconds);
            await Task.Delay(delay);
            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed after {MaxRetries} attempts", maxRetries);
            throw;
        }
    }
}

// Global exception middleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// TLS termination is handled at the ingress/reverse proxy in containerized environments
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();