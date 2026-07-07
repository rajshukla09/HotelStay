# Hotel Stay Availability

Hotel Stay Availability is a .NET 8 case study for searching hotel availability across external providers and making in-memory reservations through ASP.NET Core controllers.

## Architecture

The backend now follows a Clean Architecture layout with domain rules, application use cases, infrastructure implementations, and API hosting concerns split into separate projects.

## Repository structure

```text
hotel-stay/
├── README.md
├── spec.md
├── prompts.md
├── reflection.md
├── src/
│   ├── HotelStay.Api/
│   ├── HotelStay.Application/
│   ├── HotelStay.Domain/
│   ├── HotelStay.Infrastructure/
│   └── HotelStay.Blazor/
└── tests/
    └── HotelStay.Tests/
```

## Project responsibilities

- `HotelStay.Domain` contains domain enums and reservation entities. It has no project references.
- `HotelStay.Application` contains DTOs, CQRS commands, queries, handlers, application interfaces, and operation results. It references `HotelStay.Domain` only.
- `HotelStay.Infrastructure` contains provider stubs, the in-memory reservation store, document validation, and DI registration. It references `HotelStay.Application` and `HotelStay.Domain`.
- `HotelStay.Api` contains ASP.NET Core controllers, middleware, Swagger setup, and `Program.cs`. It references `HotelStay.Application` and `HotelStay.Infrastructure` and maps controllers with `app.MapControllers()`.
- `HotelStay.Tests` references the backend projects as needed for integration and application tests.

## Prerequisites

- .NET 8 SDK

## Restore and build

```bash
dotnet restore HotelStay.sln
dotnet build HotelStay.sln
```

## Run the API

```bash
dotnet run --project src/HotelStay.Api
```

The API launch profile opens Swagger at `http://localhost:5080/swagger` for the `http` profile or `https://localhost:7080/swagger` for the `https` profile. The API also exposes a development health endpoint at `/health` and hotel controller endpoints under `/api/hotels`.

## Run the Blazor WebAssembly app

```bash
dotnet run --project src/HotelStay.Blazor
```

## Run tests

```bash
dotnet test HotelStay.sln
```

## Documentation

- `spec.md` describes the planned problem, contracts, validation, frontend states, testing approach, and provider extensibility.
- `prompts.md` records AI usage notes for the case study.
- `reflection.md` captures placeholders for future improvement areas.
