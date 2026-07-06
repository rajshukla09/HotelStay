# Hotel Stay Availability

Hotel Stay Availability is a .NET 8 case study for searching hotel availability across external providers. This repository currently contains the initial solution structure, project scaffolding, and planning documentation only. Business logic and provider integrations are intentionally not implemented yet.

## Repository structure

```text
hotel-stay/
├── README.md
├── spec.md
├── prompts.md
├── reflection.md
├── HotelStay.Api/
├── HotelStay.Tests/
└── HotelStay.Blazor/
```

## Prerequisites

- .NET 8 SDK

## Restore and build

```bash
dotnet restore HotelStay.sln
dotnet build HotelStay.sln
```

## Run the API

```bash
dotnet run --project HotelStay.Api
```

The API currently exposes a development health endpoint at `/health`.

## Run the Blazor WebAssembly app

```bash
dotnet run --project HotelStay.Blazor
```

## Run tests

```bash
dotnet test HotelStay.sln
```

## Documentation

- `spec.md` describes the planned problem, contracts, validation, frontend states, testing approach, and provider extensibility.
- `prompts.md` records AI usage notes for the case study.
- `reflection.md` captures placeholders for future improvement areas.
