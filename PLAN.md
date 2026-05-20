# TelehealthBooking.Engine — Implementation Logbook

> **Last updated:** 2026-05-20
> **Build:** ✅ 0 errors, 0 warnings (all 5 projects)
> **Tests:** ✅ 23/23 passed
> **Docker:** ✅ Compose up --build, full lifecycle verified

---

## Original State (Before)

| Layer | Component | Status |
|---|---|---|
| Domain | `Appointment`, `Doctor`, `Patient` entities | ✅ Done |
| Domain | `BaseEntity<TId>` abstract base | ✅ Done |
| Application | `BookAppointmentCommand` + Handler | ✅ Done |
| Application | `BookAppointmentCommandValidator` | ✅ Done |
| Application | `IAppointmentRepository` (Add, HasOverlap only) | ⚠️ Partial |
| Infrastructure | `ApplicationDbContext` (3 DbSets) | ✅ Done |
| Infrastructure | `AppointmentConfiguration` (EF config) | ✅ Done |
| Infrastructure | `AppointmentRepository` (Add, HasOverlap only) | ⚠️ Partial |
| Infrastructure | Migration `InitialCreate` (3 tables) | ✅ Done |
| API | `AppointmentsController` (POST only) | ⚠️ Partial |
| API | `Program.cs` DI registration | ⚠️ Partial |
| API | `appsettings.json` (connection string) | ✅ Done |
| Tests | `BookAppointmentCommandHandlerTests` (2 files, duplicate) | ⚠️ Needs cleanup |

**Only 1 working endpoint:** `POST /api/appointments`
**Only 3 tests** (2 in one file, 1 in a duplicate)

---

## Phase 1 — Cleanup & Infrastructure Fixes

### 1.1 Delete stub files
- Removed `Class1.cs` from Domain, Application, and Infrastructure projects

### 1.2 Clean up duplicate tests
- Kept `ApplicationTests/BookAppointmentCommandHandlerTests.cs`
- Deleted `Features/Appointments/BookAppointmentHandlerTests.cs` and its empty directory
- Removed `UnitTest1.cs` template stub

### 1.3 Add EF Fluent API configs
- **`DoctorConfiguration.cs`** — `HasMaxLength(200)` on `Name` and `Specialization`, `ValueGeneratedNever()` on `Id`
- **`PatientConfiguration.cs`** — `HasMaxLength(200)` on `Name` and `Email`, `ValueGeneratedNever()` on `Id`
- Generated new migration `DoctorPatientConfigurations` to sync the model snapshot

### 1.4 Register FluentValidation pipeline
- Created **`ValidationBehavior<TRequest, TResponse>`** — `IPipelineBehavior` that auto-runs all validators before the handler
- Registered in `Program.cs`:
  - `AddValidatorsFromAssemblyContaining<BookAppointmentCommandValidator>()`
  - `AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))`

### 1.5 Add global exception middleware
- Created **`ExceptionMiddleware`** — catches `ValidationException` (structured field errors) and generic `Exception` (single error message) and returns JSON
- Registered as `app.UseMiddleware<ExceptionMiddleware>()`
- Removed `try/catch` from `AppointmentsController`

### 1.6 Update API.http
- Replaced weatherforecast template with 3 real requests: book appointment, overlap test, validation test

---

## Phase 2 — Complete Appointment CRUD

### 2.1 AppointmentDto
- Created `AppointmentDto` with `Id`, `PatientId`, `DoctorId`, `ScheduledTimeUtc`, `Status`, `CancellationReason`, `CreatedAtUtc`, `UpdatedAtUtc`

### 2.2-2.3 Expand repository
- Added to `IAppointmentRepository`: `GetByIdAsync`, `GetAllAsync`, `GetByPatientIdAsync`, `GetByDoctorIdAsync`, `UpdateAsync`, `DeleteAsync`
- Changed `HasOverlappingAppointmentAsync` signature to accept `Guid? excludeAppointmentId` (for reschedule)
- Implemented all methods in `AppointmentRepository`

### 2.4 Queries
- **`GetAppointmentByIdQuery`** — returns `AppointmentDto?` or null
- **`GetAllAppointmentsQuery`** — returns `List<AppointmentDto>`
- **`GetAppointmentsByPatientIdQuery`** — filtered by patient
- **`GetAppointmentsByDoctorIdQuery`** — filtered by doctor

### 2.5 Cancel command
- **`CancelAppointmentCommand(AppointmentId, Reason)`** — loads appointment, calls `appointment.Cancel(reason)`, saves
- Returns 404 via exception if not found

### 2.6 Reschedule command
- **`RescheduleAppointmentCommand(AppointmentId, NewScheduledTimeUtc)`** — loads appointment, rejects if cancelled, re-checks overlap excluding self, calls `appointment.Reschedule(newTime)`, saves
- Added **`Appointment.Reschedule(DateTime)`** method to the domain entity

### 2.7 Validators
- **`CancelAppointmentCommandValidator`** — `AppointmentId` not empty, `Reason` not empty, max 500 chars
- **`RescheduleAppointmentCommandValidator`** — `AppointmentId` not empty, `NewScheduledTimeUtc` must be in future

### 2.8 Controller endpoints
| Method | Route | Description |
|---|---|---|
| POST | `/api/appointments` | Book (existing, now uses global exception middleware) |
| GET | `/api/appointments` | List all |
| GET | `/api/appointments/{id}` | Get by ID |
| GET | `/api/appointments/by-patient/{patientId}` | By patient |
| GET | `/api/appointments/by-doctor/{doctorId}` | By doctor |
| PUT | `/api/appointments/{id}/cancel` | Cancel |
| PUT | `/api/appointments/{id}/reschedule` | Reschedule |

### 2.9 Tests
Consolidated into `AppointmentCommandHandlerTests.cs` — 7 tests:
- `BookAppointment_WhenNoOverlap_ShouldReturnNewAppointmentId`
- `BookAppointment_WhenOverlapExists_ShouldThrowException`
- `CancelAppointment_WhenExists_ShouldUpdateStatus`
- `CancelAppointment_WhenNotFound_ShouldThrowException`
- `RescheduleAppointment_WhenNoOverlap_ShouldUpdateTime`
- `RescheduleAppointment_WhenOverlapExists_ShouldThrowException`
- `RescheduleAppointment_WhenCancelled_ShouldThrowException`

---

## Phase 3 — Doctor CRUD

### 3.1-3.2 Repository
- **`IDoctorRepository`** interface with `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- **`DoctorRepository`** — implements all methods using EF Core

### 3.3 DoctorDto
- `DoctorDto` with `Id`, `Name`, `Specialization`, `CreatedAtUtc`, `UpdatedAtUtc`

### 3.4-3.5 Commands & Queries
- **`CreateDoctorCommand`** — creates via `Doctor.Create()` factory, returns `Guid`
- **`UpdateDoctorCommand`** — loads, calls `doctor.Update(name, specialization)`, saves
- **`DeleteDoctorCommand`** — loads, deletes
- **`GetDoctorByIdQuery`** — returns `DoctorDto?`
- **`GetAllDoctorsQuery`** — returns `List<DoctorDto>`
- Added **`Doctor.Update(string, string)`** method to the domain entity

### 3.6 Validators
- `CreateDoctorCommandValidator` — name and specialization not empty, max 200
- `UpdateDoctorCommandValidator` — same + `Id` not empty

### 3.7 Controller
| Method | Route | Description |
|---|---|---|
| POST | `/api/doctors` | Create |
| GET | `/api/doctors` | List all |
| GET | `/api/doctors/{id}` | Get by ID |
| PUT | `/api/doctors/{id}` | Update |
| DELETE | `/api/doctors/{id}` | Delete |

### 3.8 DI registration
- `builder.Services.AddScoped<IDoctorRepository, DoctorRepository>()`

### 3.9 Tests
`DoctorCommandHandlerTests.cs` — 8 tests covering all handlers (happy + error paths)

---

## Phase 4 — Patient CRUD

Same pattern as Doctor:

### 4.1-4.2 Repository
- **`IPatientRepository`**, **`PatientRepository`**

### 4.3 PatientDto
- `PatientDto` with `Id`, `Name`, `Email`, `CreatedAtUtc`, `UpdatedAtUtc`

### 4.4-4.5 Commands & Queries
- `CreatePatientCommand`, `UpdatePatientCommand`, `DeletePatientCommand`
- `GetPatientByIdQuery`, `GetAllPatientsQuery`
- Added **`Patient.Update(string, string)`** method to the domain entity

### 4.6 Validators
- Name not empty, email format validation

### 4.7 Controller
| Method | Route | Description |
|---|---|---|
| POST | `/api/patients` | Create |
| GET | `/api/patients` | List all |
| GET | `/api/patients/{id}` | Get by ID |
| PUT | `/api/patients/{id}` | Update |
| DELETE | `/api/patients/{id}` | Delete |

### 4.9 Tests
`PatientCommandHandlerTests.cs` — 8 tests covering all handlers (happy + error paths)

---

## Docker Setup

### docker-compose.yml
```yaml
services:
  sqlserver:   # SA_PASSWORD, healthcheck, persistent volume
  api:         # .NET 9, auto-migration, depends_on sqlserver healthy
```

### Program.cs changes for Docker
- **Auto-migration on startup** — `db.Database.MigrateAsync()` with retry (5 attempts, exponential backoff 3s→6s→12s→24s→48s)
- **Conditional HTTPS redirect** — skipped when `ASPNETCORE_ENVIRONMENT=Docker`
- **Conditional OpenAPI/Scalar** — served in both `Development` and `Docker` environments

### To run
```bash
docker compose up --build
# API at http://localhost:8080
# Scalar docs at http://localhost:8080/scalar/v1
```

---

## Full API Lifecycle Test Results (Docker)

All 14 steps verified end-to-end:

| Step | Endpoint | Expected | Actual |
|---|---|---|---|
| 1 | `POST /api/doctors` | 201 + GUID | ✅ |
| 2 | `POST /api/patients` | 201 + GUID | ✅ |
| 3 | `GET /api/doctors`, `/patients` | 200 + arrays | ✅ |
| 4 | `GET /api/doctors/{id}`, `/patients/{id}` | 200 + object | ✅ |
| 5 | `POST /api/appointments` | 201 + GUID | ✅ |
| 6 | `POST /api/appointments` (overlap) | 400 + conflict | ✅ |
| 7 | `POST /api/appointments` (invalid) | 400 + validation | ✅ |
| 8 | `GET /api/appointments/{id}` | 200 + Pending | ✅ |
| 9 | `GET /api/appointments` + filters | 200 + 1 each | ✅ |
| 10 | `PUT .../reschedule` | 204 + time updated | ✅ |
| 11 | `PUT .../cancel` | 204 + Cancelled | ✅ |
| 12 | `PUT /api/doctors/{id}` | 204 + updated | ✅ |
| 13 | `PUT /api/patients/{id}` | 204 + updated | ✅ |
| 14 | `DELETE /api/doctors/{id}`, `/patients/{id}` | 204, then 404 | ✅ |

---

## Final Project Structure

```
TelehealthBooking.sln
├── TelehealthBooking.Domain/
│   └── Entities/
│       ├── BaseEntity.cs            # Abstract base with Id, timestamps
│       ├── Appointment.cs           # Create, Cancel, Confirm, Reschedule
│       ├── Doctor.cs                # Create, Update
│       └── Patient.cs               # Create, Update
│
├── TelehealthBooking.Application/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs    # FluentValidation pipeline
│   ├── DTOs/
│   │   ├── AppointmentDto.cs
│   │   ├── DoctorDto.cs
│   │   └── PatientDto.cs
│   ├── Features/
│   │   ├── Appointments/
│   │   │   ├── Commands/
│   │   │   │   ├── BookAppointmentCommand.cs         + Handler
│   │   │   │   ├── BookAppointmentCommandValidator.cs
│   │   │   │   ├── CancelAppointmentCommand.cs       + Handler
│   │   │   │   ├── CancelAppointmentCommandValidator.cs
│   │   │   │   ├── RescheduleAppointmentCommand.cs   + Handler
│   │   │   │   └── RescheduleAppointmentCommandValidator.cs
│   │   │   └── Queries/
│   │   │       ├── GetAppointmentByIdQuery.cs
│   │   │       ├── GetAllAppointmentsQuery.cs
│   │   │       ├── GetAppointmentsByPatientIdQuery.cs
│   │   │       └── GetAppointmentsByDoctorIdQuery.cs
│   │   ├── Doctors/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateDoctorCommand.cs            + Handler + Validator
│   │   │   │   ├── UpdateDoctorCommand.cs            + Handler + Validator
│   │   │   │   └── DeleteDoctorCommand.cs            + Handler
│   │   │   └── Queries/
│   │   │       ├── GetDoctorByIdQuery.cs
│   │   │       └── GetAllDoctorsQuery.cs
│   │   └── Patients/
│   │       ├── Commands/
│   │       │   ├── CreatePatientCommand.cs            + Handler + Validator
│   │       │   ├── UpdatePatientCommand.cs            + Handler + Validator
│   │       │   └── DeletePatientCommand.cs            + Handler
│   │       └── Queries/
│   │           ├── GetPatientByIdQuery.cs
│   │           └── GetAllPatientsQuery.cs
│   └── Interfaces/
│       ├── IAppointmentRepository.cs
│       ├── IDoctorRepository.cs
│       └── IPatientRepository.cs
│
├── TelehealthBooking.Infrastructure/
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       ├── Configurations/
│       │   ├── AppointmentConfiguration.cs
│       │   ├── DoctorConfiguration.cs
│       │   └── PatientConfiguration.cs
│       ├── Migrations/
│       │   ├── 20260228105552_InitialCreate.cs
│       │   ├── 20260228105552_InitialCreate.Designer.cs
│       │   ├── 20260520064532_DoctorPatientConfigurations.cs   (NEW)
│       │   ├── 20260520064532_DoctorPatientConfigurations.Designer.cs
│       │   └── ApplicationDbContextModelSnapshot.cs
│       └── Repositories/
│           ├── AppointmentRepository.cs
│           ├── DoctorRepository.cs
│           └── PatientRepository.cs
│
├── TelehealthBooking.Api/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs  # 7 endpoints
│   │   ├── DoctorsController.cs       # 5 endpoints
│   │   └── PatientsController.cs      # 5 endpoints
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   ├── Program.cs                     # DI, auto-migration, Scalar, middleware
│   └── TelehealthBooking.Api.http
│
├── TelehealthBooking.Tests/
│   └── ApplicationTests/
│       ├── AppointmentCommandHandlerTests.cs  # 7 tests
│       ├── DoctorCommandHandlerTests.cs       # 8 tests
│       └── PatientCommandHandlerTests.cs      # 8 tests
│
├── docker-compose.yml     # SQL Server + API
├── Dockerfile             # Multi-stage .NET 9 build
├── PLAN.md                # This file
└── README.md              # Updated with full documentation
```

---

## Remaining (Phase 5 — Optional)

| Task | Priority |
|---|---|
| Pagination on list endpoints | Low |
| Filtering by date range / status | Low |
| `ConfirmAppointmentCommand` endpoint | Low |
| Custom domain exceptions (not plain `Exception`) | Medium |
| Integration tests with in-memory DB | Medium |
| JWT authentication + role-based access | Low |
