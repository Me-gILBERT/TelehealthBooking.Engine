# TelehealthBooking.Engine

A RESTful appointment booking API built with **.NET 9** and **Clean Architecture**, designed to manage the full lifecycle of healthcare appointments across three roles: Doctor, Patient, and Administrator.

Built as a portfolio project to demonstrate enterprise backend patterns including CQRS, dependency inversion, defensive validation, and automated testing.

**Build:** ✅ 0 errors · **Tests:** ✅ 23/23 pass · **Docker:** ✅ Single `docker compose up --build`

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
| Validation | FluentValidation (via MediatR pipeline behavior) |
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
├── TelehealthBooking.Domain          # Entities, business rules, no external deps
│   └── Entities/                     # Appointment, Doctor, Patient, BaseEntity
│
├── TelehealthBooking.Application     # CQRS handlers, interfaces, validation
│   ├── Behaviors/                    # ValidationBehavior (FluentValidation pipeline)
│   ├── DTOs/                         # AppointmentDto, DoctorDto, PatientDto
│   ├── Features/                     # Appointments/, Doctors/, Patients/
│   │   ├── Commands/                 # Book, Cancel, Reschedule, Create, Update, Delete
│   │   └── Queries/                  # GetById, GetAll, GetByPatient, GetByDoctor
│   └── Interfaces/                   # IAppointmentRepository, IDoctorRepository, IPatientRepository
│
├── TelehealthBooking.Infrastructure  # EF Core, SQL Server, repositories
│   ├── Persistence/
│   │   ├── Configurations/           # Fluent API configs for all 3 entities
│   │   ├── Migrations/               # InitialCreate + DoctorPatientConfigurations
│   │   └── Repositories/            # AppointmentRepository, DoctorRepository, PatientRepository
│
├── TelehealthBooking.Api             # ASP.NET Core host
│   ├── Controllers/                  # AppointmentsController (7), DoctorsController (5), PatientsController (5)
│   ├── Middleware/                    # ExceptionMiddleware (global error handling)
│   └── Program.cs                    # DI, auto-migration, Scalar, middleware pipeline
│
└── TelehealthBooking.Tests           # 23 unit tests
    └── ApplicationTests/             # AppointmentCommandHandlerTests (7), DoctorCommandHandlerTests (8), PatientCommandHandlerTests (8)
```

**The core rule:** The `Domain` layer has zero external package dependencies. Business logic doesn't know that EF Core or SQL Server exist.

---

## Key Design Decisions

### Clean Architecture — Why?
Each layer has one job. The Controller doesn't touch the database. The Repository doesn't know about HTTP. If tomorrow the team decides to switch from SQL Server to PostgreSQL, only `Infrastructure` changes — nothing else.

### CQRS with MediatR — Why?
Read and write operations are separated into Commands (writes) and Queries (reads). The API controllers are kept thin — they receive the HTTP request, send it to MediatR, and return the result. All business logic lives in the handlers, not in the controllers.

### FluentValidation as a Pipeline Behavior — Why?
A `ValidationBehavior<TRequest, TResponse>` registered as a MediatR `IPipelineBehavior` runs automatically *before* any handler executes. If a request violates a business rule (e.g., booking an appointment in the past, or sending empty IDs), it is rejected with a `400 Bad Request` without touching the database. This is the "Defensive Layer."

### Global Exception Middleware — Why?
A single `ExceptionMiddleware` catches all unhandled exceptions and returns structured JSON error responses. Controllers no longer need per-action try/catch blocks.

### Repository Pattern + Dependency Inversion — Why?
The `Application` layer defines *what* it needs via interfaces (e.g., `IAppointmentRepository`). The `Infrastructure` layer provides the actual implementation. This means unit tests can swap in a fake (mocked) repository without needing a real database connection.

### DTO Pattern — Why?
Domain entities and API responses are intentionally different objects. Sensitive fields like `PasswordHash` exist on the entity but are never exposed in API responses. DTOs control exactly what data crosses the API boundary.

---

## Core Domain: The Appointment Lifecycle

An `Appointment` entity enforces its own state transitions through methods:

```csharp
appointment.Confirm();            // Status: "Pending" → "Confirmed"
appointment.Cancel(reason);       // Status: any → "Cancelled"
appointment.Reschedule(newTime);  // Updates ScheduledTimeUtc, re-checks overlap
```

Properties use `private set` — external code cannot directly mutate appointment state. State changes only happen through these controlled methods.

---

## Conflict Detection

The booking and reschedule handlers check for overlapping appointments before saving:

```
A doctor cannot have two active (non-cancelled) appointments within a 30-minute window.
If overlap is detected → exception thrown via middleware → 400 Bad Request.
```

When rescheduling, the overlap check excludes the appointment being rescheduled to avoid false conflicts.

This business rule is enforced in the handler, tested in isolation via mocked repositories.

---

## API Endpoints

### Appointments
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/appointments` | Book a new appointment |
| GET | `/api/appointments` | List all appointments |
| GET | `/api/appointments/{id}` | Get appointment by ID |
| GET | `/api/appointments/by-patient/{patientId}` | Get appointments for a patient |
| GET | `/api/appointments/by-doctor/{doctorId}` | Get appointments for a doctor |
| PUT | `/api/appointments/{id}/cancel` | Cancel an appointment |
| PUT | `/api/appointments/{id}/reschedule` | Reschedule an appointment |

### Doctors
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/doctors` | Create a doctor |
| GET | `/api/doctors` | List all doctors |
| GET | `/api/doctors/{id}` | Get doctor by ID |
| PUT | `/api/doctors/{id}` | Update a doctor |
| DELETE | `/api/doctors/{id}` | Delete a doctor |

### Patients
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/patients` | Create a patient |
| GET | `/api/patients` | List all patients |
| GET | `/api/patients/{id}` | Get patient by ID |
| PUT | `/api/patients/{id}` | Update a patient |
| DELETE | `/api/patients/{id}` | Delete a patient |

API documentation is available at `/scalar/v1` when running in Development or Docker mode.

---

## Getting Started

### Option A: Docker (recommended — no dependencies needed)

```bash
git clone https://github.com/Me-gILBERT/TelehealthBooking.Engine.git
cd TelehealthBooking.Engine
docker compose up --build
```

Open `http://localhost:8080/scalar/v1` to explore the API. The database, tables, and seed data are all handled automatically.

### Option B: Local .NET + SQL Server

```bash
# Prerequisites
# - .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0
# - SQL Server or LocalDB
# - dotnet tool install --global dotnet-ef

git clone https://github.com/Me-gILBERT/TelehealthBooking.Engine.git
cd TelehealthBooking.Engine

dotnet restore

dotnet ef database update \
  --project TelehealthBooking.Infrastructure/TelehealthBooking.Infrastructure.csproj \
  --startup-project TelehealthBooking.Api/TelehealthBooking.Api.csproj

dotnet run --project TelehealthBooking.Api/TelehealthBooking.Api.csproj
```

Open `https://localhost:<port>/scalar/v1` to explore the API.

### Run Tests

```bash
dotnet test
```
Tests run entirely in-memory using mocked repositories — 23 tests, 0 database required.

---

## Full API Lifecycle Test

These commands walk through a complete booking lifecycle targeting the Docker setup (`http://localhost:8080`). Save the returned GUIDs as environment variables for subsequent requests.

### 1. Create a Doctor
```bash
curl -s -X POST http://localhost:5279/api/doctors \
  -H "Content-Type: application/json" \
  -d '{"name":"Dr. Sarah Chen","specialization":"Cardiology"}'
# → 201, returns doctor GUID → save as $DOCTOR_ID
```

### 2. Create a Patient
```bash
curl -s -X POST http://localhost:5279/api/patients \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'
# → 201, returns patient GUID → save as $PATIENT_ID
```

### 3. Book an Appointment
```bash
curl -s -X POST http://localhost:5279/api/appointments \
  -H "Content-Type: application/json" \
  -d "{\"patientId\":\"$PATIENT_ID\",\"doctorId\":\"$DOCTOR_ID\",\"scheduledTimeUtc\":\"2026-06-15T14:00:00Z\"}"
# → 201, returns appointment GUID → save as $APPT_ID
```

### 4. Test Overlap Detection
```bash
curl -s -X POST http://localhost:5279/api/appointments \
  -H "Content-Type: application/json" \
  -d "{\"patientId\":\"$PATIENT_ID\",\"doctorId\":\"$DOCTOR_ID\",\"scheduledTimeUtc\":\"2026-06-15T14:15:00Z\"}"
# → 400: "Doctor is already booked for this time slot."
```

### 5. Test Validation
```bash
curl -s -X POST http://localhost:5279/api/appointments \
  -H "Content-Type: application/json" \
  -d '{"patientId":"","doctorId":"00000000-0000-0000-0000-000000000000","scheduledTimeUtc":"2020-01-01T00:00:00Z"}'
# → 400: "Validation failed" with field-level details
```

### 6. Query Appointments
```bash
curl -s http://localhost:5279/api/appointments/$APPT_ID           | jq   # Get by ID
curl -s http://localhost:5279/api/appointments                    | jq   # List all
curl -s http://localhost:5279/api/appointments/by-doctor/$DOCTOR_ID | jq # By doctor
curl -s http://localhost:5279/api/appointments/by-patient/$PATIENT_ID | jq # By patient
```

### 7. Reschedule
```bash
curl -s -X PUT http://localhost:5279/api/appointments/$APPT_ID/reschedule \
  -H "Content-Type: application/json" \
  -d '{"newScheduledTimeUtc":"2026-06-16T10:00:00Z"}'
# → 204; verify with GET → scheduledTimeUtc updated
```

### 8. Cancel
```bash
curl -s -X PUT http://localhost:5279/api/appointments/$APPT_ID/cancel \
  -H "Content-Type: application/json" \
  -d '{"reason":"Patient requested cancellation"}'
# → 204; verify with GET → status "Cancelled"
```

### 9. Update & Delete Resources
```bash
# Update doctor
curl -s -X PUT http://localhost:5279/api/doctors/$DOCTOR_ID \
  -H "Content-Type: application/json" \
  -d "{\"id\":\"$DOCTOR_ID\",\"name\":\"Dr. Sarah Chen\",\"specialization\":\"Neurology\"}"
# → 204

# Delete patient
curl -s -X DELETE http://localhost:5279/api/patients/$PATIENT_ID
# → 204; verify GET → 404
```

---

## Testing Approach

Unit tests follow the **AAA pattern** (Arrange, Act, Assert) with **23 tests** covering:

- **Appointment handlers** — booking (happy + overlap), cancellation (exists + not found), rescheduling (happy + overlap + cancelled)
- **Doctor handlers** — create, get by ID (exists + not found), list all, update (exists + not found), delete (exists + not found)
- **Patient handlers** — create, get by ID (exists + not found), list all, update (exists + not found), delete (exists + not found)

Each test proves both that the system works correctly *and* that it defends itself against invalid states (e.g., `AddAsync` is *never* called when an overlap exists — verified via `Times.Never`).

---

## Docker

A `docker-compose.yml` orchestrates two services: the API and SQL Server 2022. Migrations apply automatically on startup — no manual `dotnet ef database update` needed.

```bash
# Build and run everything
docker compose up --build
```

The API becomes available at `http://localhost:8080` and Scalar docs at `http://localhost:8080/scalar/v1`.

### Docker Compose stack

| Service | Image | Purpose |
|---|---|---|
| `api` | Built from `Dockerfile` (multi-stage .NET 9 build) | ASP.NET Core API |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | SQL Server database |

### Key Docker features

- **Health check** — the API waits for SQL Server to be ready before starting (`service_healthy` condition)
- **Auto-migration** — `db.Database.MigrateAsync()` runs on startup with retry logic (5 attempts, exponential backoff)
- **Persistent data** — SQL Server data is stored in a named volume (`sqlserver_data`) and survives container restarts
- **No HTTPS redirect** — TLS termination is handled at the reverse proxy/ingress level, not inside the container
- **Connection string** — configured via environment variable (`ConnectionStrings__DefaultConnection`) overriding `appsettings.json`

### Query the database (Docker)

You can connect to SQL Server with any database tool (SSMS, Azure Data Studio, DBeaver, etc.):

| Field | Value |
|---|---|
| Host | `localhost` |
| Port | `1433` |
| Database | `TelehealthBookingDb` |
| Username | `sa` |
| Password | `Telehealth@2026` |
| SSL | Disable or trust server certificate |

Or from the command line:

```bash
docker exec -it telehealthbookingengine-sqlserver-1 \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Telehealth@2026" -C \
  -Q "SELECT Id, PatientId, DoctorId, ScheduledTimeUtc, Status FROM TelehealthBookingDb.dbo.Appointments"
```

---

## CI Pipeline

A GitHub Actions workflow runs on every push to `main`:

1. Provisions an Ubuntu runner
2. Installs .NET 9
3. Restores dependencies
4. Builds the solution in Release mode
5. Runs the full unit test suite (23 tests)

If any test fails, the pipeline blocks the merge.

---

## Project Status

| Module | Status |
|---|---|
| Clean Architecture setup | ✅ Complete |
| Core domain entities + state transitions | ✅ Complete |
| Appointment CRUD (book, list, get, cancel, reschedule) | ✅ Complete |
| Doctor CRUD (create, read, update, delete) | ✅ Complete |
| Patient CRUD (create, read, update, delete) | ✅ Complete |
| FluentValidation pipeline behavior | ✅ Complete |
| Global exception middleware | ✅ Complete |
| EF Core + SQL Server persistence | ✅ Complete |
| REST API + Scalar documentation | ✅ Complete |
| Unit testing (23 tests, xUnit + Moq) | ✅ Complete |
| Docker containerisation | ✅ Complete |
| GitHub Actions CI | ✅ Complete |
| Pagination / filtering | 🔲 Planned |
| JWT Authentication | 🔲 Planned |

---

## Author

**Gilbert Nathaniel**
[LinkedIn](https://www.linkedin.com/in/gilbert-nathaniel-905a711a9) · [GitHub](https://github.com/Me-gILBERT)
