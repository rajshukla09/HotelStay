# Hotel Stay Availability

## Project summary

Hotel Stay Availability is a .NET 8 case study for searching hotel availability, validating travel documents, and creating in-memory reservations. The solution includes an ASP.NET Core backend, a Blazor WebAssembly frontend, and two deterministic stub hotel providers that make the behavior repeatable for testing and review.

## Architecture

The backend follows a concise Clean Architecture structure: domain rules live in the Domain project, application use cases and contracts live in Application, deterministic provider and persistence implementations live in Infrastructure, and HTTP hosting concerns live in Api. The design keeps provider integration behind `IHotelProvider`, so a third provider can be added without changing the API surface.

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
- `HotelStay.Infrastructure` contains provider stubs, the in-memory reservation store, document validation, and dependency injection registration. It references `HotelStay.Application` and `HotelStay.Domain`.
- `HotelStay.Api` contains ASP.NET Core controllers, middleware, Swagger setup, and `Program.cs`. It references `HotelStay.Application` and `HotelStay.Infrastructure`.
- `HotelStay.Blazor` contains the Blazor WebAssembly user interface.
- `HotelStay.Tests` references the backend projects as needed for integration and application tests.

## Prerequisites

- .NET 8 SDK

## Local URLs

- API Swagger: <https://localhost:7080/swagger>
- Blazor UI: <http://localhost:5200>

## Supported destinations

Domestic destinations:

- Sydney
- Melbourne

International destinations:

- London
- Paris
- Tokyo

Unknown destinations return no rooms.

## Run instructions

From a clean clone, run:

```bash
dotnet restore HotelStay.sln
dotnet build HotelStay.sln
dotnet test HotelStay.sln
dotnet run --project src/HotelStay.Api
dotnet run --project src/HotelStay.Blazor
```

The API exposes Swagger, a development health endpoint at `/health`, and hotel endpoints under `/api/hotels`.

## API examples

Search availability:

```http
GET /api/hotels/search?destination=Sydney&checkIn=2026-07-08&checkOut=2026-07-10
```

Create a reservation:

```http
POST /api/hotels/reserve
Content-Type: application/json

{
  "roomId": "PremierStays-Sydney-ps-standard",
  "provider": "PremierStays",
  "destination": "Sydney",
  "checkIn": "2026-07-08",
  "checkOut": "2026-07-10",
  "roomType": "Standard",
  "perNightRate": 145,
  "totalPrice": 290,
  "guestName": "Alex Guest",
  "documentType": "NationalId",
  "documentNumber": "NAT-123456",
  "cancellationPolicy": "FreeCancellation48Hours"
}
```

Domestic destinations accept `NationalId` or `Passport`. International destinations require `Passport`. Invalid document and unsupported destination mismatches return `422 Unprocessable Entity`.

## Assumptions

- No database; reservations are stored in memory.
- No authentication.
- No real provider integrations.
- Provider stubs are deterministic.
- Unknown destinations return no rooms.
- Prices are simple decimal values.
- The solution is designed so a third provider can be added through `IHotelProvider`.

## Documentation

- `spec.md` describes the planned problem, contracts, validation, frontend states, testing approach, and provider extensibility.
- `prompts.md` records AI usage notes for the case study.
- `reflection.md` captures improvement areas if more time were available.

## Final verification checklist

- API builds and runs.
- UI builds and runs.
- Tests pass.
- Search flow works.
- Reservation flow works.
- `422` document validation works.
- Reservation lookup works.
