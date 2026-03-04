# TelehealthBooking.Engine
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20Onion-blue.svg)]()
[![Build & Test](https://github.com/yourusername/TelehealthBooking.Engine/actions/workflows/ci.yml/badge.svg)](https://github.com/yourusername/TelehealthBooking.Engine/actions/workflows/ci.yml)

An enterprise-grade, distributed RESTful API designed to manage telehealth medical appointments. This system is engineered using **.NET 9** and serves as a robust implementation of **Clean Architecture** and **CQRS**, heavily decoupling business logic from external frameworks to ensure maximum maintainability, testability, and scalability.

## 🚀 Tech Stack & Frameworks

| Category | Technology / Library | Purpose |
| :--- | :--- | :--- |
| **Core Framework** | .NET 9 (C# 13) | Base runtime and language features |
| **Architecture** | Clean Architecture & CQRS | Separation of concerns and use-case isolation |
| **Routing & Logic** | MediatR | Decoupling API controllers from business logic |
| **Data Integrity** | FluentValidation | Pre-execution defensive data validation |
| **Persistence** | Entity Framework Core 9 | ORM and Code-First Database Migrations |
| **Database** | SQL Server (LocalDB) | Relational data storage |
| **API UI** | Scalar / OpenAPI | Modern, interactive API documentation |
| **Testing** | xUnit, Moq, FluentAssertions | Unit testing and dependency mocking |
| **DevOps** | Docker & GitHub Actions | Containerization and automated CI/CD pipeline |

## 🏗️ Architectural Highlights

### 1. Clean Architecture Strictness
The solution is divided into strictly enforced, dependency-inverted layers. The `Domain` layer sits at the absolute center with **zero external dependencies**. The `Application` layer handles use cases, while the `Infrastructure` and `Api` layers are treated as volatile external details.

### 2. Command Query Responsibility Segregation (CQRS)
Using **MediatR**, read and write operations are strictly segregated. API Controllers act merely as entry points, remaining incredibly thin and delegating all business logic to dedicated Command or Query Handlers.

### 3. The Defensive Pipeline
Data integrity is enforced *before* it ever reaches the database or the business logic. By implementing **FluentValidation** as a MediatR pipeline behavior, invalid requests (e.g., attempting to book an appointment in the past) are intercepted at the application boundary and rejected with a `400 Bad Request`.

### 4. Rich Domain Models
Domain entities (like `Appointment`) encapsulate their own business rules. Properties utilize `private set;` modifiers, meaning state changes can only occur through dedicated domain methods. This prevents external layers from illegally modifying entity states.

## 📂 Solution Structure

```text
TelehealthBooking.sln
├── TelehealthBooking.Domain         # Core entities, Enums, and custom Domain Exceptions
├── TelehealthBooking.Application    # MediatR Commands/Queries, Interfaces, Validation Rules
├── TelehealthBooking.Infrastructure # EF Core DbContext, Repositories, Fluent API Configurations
├── TelehealthBooking.Api            # API Controllers, Dependency Injection Setup, Scalar UI
└── TelehealthBooking.Tests          # xUnit Tests, Moq Repositories, FluentAssertions

```

## ⚙️ Getting Started (Local Development)

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* Entity Framework Core CLI (`dotnet tool install --global dotnet-ef`)
* Docker Desktop (Optional, for containerized running)

### 1. Clone the Repository

```bash
git clone [https://github.com/yourusername/TelehealthBooking.Engine.git](https://github.com/yourusername/TelehealthBooking.Engine.git)
cd TelehealthBooking.Engine

```

### 2. Apply Database Migrations

The project is configured to use SQL Server LocalDB out of the box. Generate the schema by running:

```bash
dotnet ef database update --project TelehealthBooking.Infrastructure/TelehealthBooking.Infrastructure.csproj --startup-project TelehealthBooking.Api/TelehealthBooking.Api.csproj

```

### 3. Run the API

Launch the application locally:

```bash
dotnet run --project TelehealthBooking.Api/TelehealthBooking.Api.csproj

```

Once running, navigate to `https://localhost:<port>/scalar/v1` in your browser to access the interactive OpenAPI documentation and test the endpoints.

## 🧪 Testing

The project includes a comprehensive suite of isolated Unit Tests targeting the Application Layer handlers.

Run the test suite via the CLI:

```bash
dotnet test

```

## 🐳 Docker Support

To run the API entirely inside an isolated Linux container:

```bash
docker build -t telehealth-api .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development telehealth-api

```

Navigate to `http://localhost:8080/scalar/v1` to access the application.

## 🔄 Continuous Integration (CI)

This repository utilizes **GitHub Actions**. Every push or pull request to the `main` branch automatically triggers a workflow that provisions an Ubuntu runner, sets up the .NET 9 SDK, restores dependencies, builds the solution, and executes the xUnit test suite to prevent regressions.

