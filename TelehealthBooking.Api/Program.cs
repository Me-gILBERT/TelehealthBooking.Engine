using Microsoft.EntityFrameworkCore;
using TelehealthBooking.Application.Features.Appointments.Commands;
using TelehealthBooking.Application.Interfaces;
using TelehealthBooking.Infrastructure.Persistence;
using TelehealthBooking.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore; // <-- 1. Add this using statement

var builder = WebApplication.CreateBuilder(args);

// Add MediatR 
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<BookAppointmentCommand>());

// Add Database Context 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories 
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Add Controllers
builder.Services.AddControllers();

// 2. REPLACE SwaggerGen with the new OpenApi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 3. REPLACE UseSwagger and UseSwaggerUI with Scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();