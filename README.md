# TelehealthBooking.Engine

A RESTful appointment booking API built with **.NET 9** and **Clean Architecture**, designed to manage the full lifecycle of healthcare appointments across three roles: Doctor, Patient, and Administrator.

Built as a portfolio project to demonstrate enterprise backend patterns including CQRS, dependency inversion, defensive validation, and automated testing.

---

## Why This Project Exists

Manual appointment scheduling in healthcare creates real operational problems — no centralised availability view, no conflict detection, and no audit trail. This API simulates the backend engine of a digital health platform that solves those problems through structured, role-based data access and enforced business rules.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 9), C# 13 |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API) |
| CQRS | MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core 9 (Code-First) |
| Database | SQL Server (LocalDB) |
| API Docs | Scalar / OpenAPI |
| Testing | xUnit, Moq, FluentAssertions |
| DevOps | Docker (multi-stage build), GitHub Actions CI |

---

## Architecture

The solution is divided into four strictly enforced layers. Each layer only knows about the layer directly below it — never above.

```
TelehealthBooking.sln
├── TelehealthBooking.Domain          # Entities, business rules, no external dependencies
├── TelehealthBooking.Application     # CQRS handlers, interfaces, validation pipeline
├── TelehealthBooking.Infrastructure  # EF Core DbContext, repositories, Fluent API configs
├── TelehealthBooking.Api             # Controllers, DI wiring, Scalar UI, middleware
└── TelehealthBooking.Tests           # Unit tests — xUnit, Moq, FluentAssertions
```

**The core rule:** The `Domain` layer has zero external package dependencies. Business logic doesn't know that EF Core or SQL Server exist.

---

## Key Design Decisions

### Clean Architecture — Why?
Each layer has one job. The Controller doesn't touch the database. The Repository doesn't know about HTTP. If tomorrow the team decides to switch from SQL Server to PostgreSQL, only `Infrastructure` changes — nothing else.

### CQRS with MediatR — Why?
Read and write operations are separated into Commands (writes) and Queries (reads). The API controllers are kept thin — they receive the HTTP request, send it to MediatR, and return the result. All business logic lives in the handlers, not in the controllers.

### FluentValidation as a Pipeline Behavior — Why?
Validation runs automatically *before* any handler executes. If a request violates a business rule (e.g., booking an appointment in the past, or sending empty IDs), it is rejected with a `400 Bad Request` without touching the database. This is the "Defensive Layer."

### Repository Pattern + Dependency Inversion — Why?
The `Application` layer defines *what* it needs via interfaces (e.g., `IAppointmentRepository`). The `Infrastructure` layer provides the actual implementation. This means unit tests can swap in a fake (mocked) repository without needing a real database connection.

### DTO Pattern — Why?
Domain entities and API responses are intentionally different objects. Sensitive fields like `PasswordHash` exist on the entity but are never exposed in API responses. DTOs control exactly what data crosses the API boundary.

---

## Core Domain: The Appointment Lifecycle

An `Appointment` entity enforces its own state transitions through methods:

```csharp
appointment.Confirm();   // Status: "Pending" → "Confirmed"
appointment.Cancel(reason); // Status: any → "Cancelled"
```

Properties use `private set` — external code cannot directly mutate appointment state. State changes only happen through these controlled methods.

---

## Conflict Detection

The booking handler checks for overlapping appointments before saving:

```
A doctor cannot have two active appointments within a 30-minute window.
If overlap is detected → exception thrown → database never touched.
```

This business rule is enforced in the handler, tested in isolation via mocked repositories.

---

## API Endpoints

### Appointments
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/appointments` | Book a new appointment |

Additional CRUD endpoints (GET, PUT, DELETE) are planned as part of the Query layer expansion.

### Doctors & Patients
Standard CRUD endpoints for `Doctor` and `Patient` entities following the same pattern.

API documentation is available at `/scalar/v1` when running in Development mode.

---

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server or LocalDB
- `dotnet tool install --global dotnet-ef`

### Setup

```bash
# Clone the repository
git clone https://github.com/Me-gILBERT/TelehealthBooking.Engine.git
cd TelehealthBooking.Engine

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update \
  --project TelehealthBooking.Infrastructure/TelehealthBooking.Infrastructure.csproj \
  --startup-project TelehealthBooking.Api/TelehealthBooking.Api.csproj

# Run the API
dotnet run --project TelehealthBooking.Api/TelehealthBooking.Api.csproj
```

Open `https://localhost:<port>/scalar/v1` to explore the API.

### Run Tests

```bash
dotnet test
```

Tests run entirely in-memory using mocked repositories — no database connection required.

---

## Testing Approach

Unit tests follow the **AAA pattern** (Arrange, Act, Assert):

**Happy path** — proves a valid booking request results in exactly one `AddAsync` call to the repository.

**Negative path** — proves that when an overlapping appointment exists, the handler throws an exception and `AddAsync` is *never* called (`Times.Never`).

This means the test suite proves both that the system works correctly *and* that it defends itself against invalid states.

---

## Docker

A multi-stage `Dockerfile` is included. The build stage uses the full .NET 9 SDK; the final image uses only the lightweight runtime.

```bash
# Build the image
docker build -t telehealth-api .

# Run the container
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development telehealth-api
```

---

## CI Pipeline

A GitHub Actions workflow runs on every push to `main`:

1. Provisions an Ubuntu runner
2. Installs .NET 9
3. Restores dependencies
4. Builds the solution in Release mode
5. Runs the full unit test suite

If any test fails, the pipeline blocks the merge.

---

## Project Status

| Module | Status |
|---|---|
| Clean Architecture setup | ✅ Complete |
| Core domain entities + CQRS | ✅ Complete |
| FluentValidation defensive layer | ✅ Complete |
| EF Core + SQL Server persistence | ✅ Complete |
| REST API + Scalar documentation | ✅ Complete |
| Unit testing (xUnit + Moq) | ✅ Complete |
| Docker containerisation | ✅ Complete |
| GitHub Actions CI | ✅ Complete |
| JWT Authentication | 🔲 Planned |
| Query endpoints (GET by filters) | 🔲 Planned |

---

## Author

**Gilbert Nathaniel**
[LinkedIn](https://www.linkedin.com/in/gilbert-nathaniel-905a711a9) · [GitHub](https://github.com/Me-gILBERT)
