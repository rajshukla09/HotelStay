# AI Usage Notes

## Overview

AI assistance was used throughout the software development lifecycle for analysis, architecture, design, implementation, testing, and documentation. The objective was to translate the case study requirements into a clean, maintainable .NET 8 solution while keeping the implementation appropriate for the scope of the assessment.

AI was used as a productivity and design aid. All generated code, architectural decisions, validation logic, tests, and documentation were manually reviewed, refined, tested, and validated before submission.

---

## Analysis

AI was used to:

- Break the case study into manageable implementation tasks.
- Identify functional and non-functional requirements.
- Define the API surface.
- Plan the overall implementation sequence.
- Identify assumptions where the requirements intentionally left implementation details open.

The analysis focused on:

- Hotel search
- Provider normalization
- Reservation flow
- Reservation lookup
- Document validation
- Validation scenarios
- Testing strategy
- Documentation requirements

---

## Architecture

AI was used to explore and refine several architectural approaches before implementation.

The final solution adopted:

- Clean Architecture
- ASP.NET Core Controllers
- Lightweight CQRS
- Dependency Injection
- SOLID principles
- Provider abstraction through `IHotelProvider`
- Shared destination catalog
- In-memory reservation store

The solution was intentionally structured so additional hotel providers can be introduced with minimal changes to the core application.

---

## Backend Implementation

AI assisted with:

- Search endpoint implementation
- Provider abstraction
- Provider response normalization
- Reservation workflow
- Reservation lookup
- Request validation
- Document validation
- Global exception handling
- Swagger/OpenAPI configuration
- Dependency Injection
- In-memory reservation storage

The generated implementation was reviewed and refined to ensure it aligned with the case study requirements.

---

## Blazor UI

AI was used to assist with:

- Hotel search page
- Search results
- Reservation form
- Reservation confirmation
- Reservation lookup
- Client-side validation
- API client abstraction
- Configuration-driven API endpoint
- User experience improvements

The UI was then manually tested end-to-end against the backend APIs.

---

## Testing

AI assisted with identifying meaningful test scenarios including:

- Search validation
- Date validation
- Provider normalization
- Total price calculation
- Room filtering
- Reservation creation
- Reservation lookup
- Document validation
- Unknown destination behavior
- End-to-end Swagger verification

Generated tests and scenarios were manually reviewed and executed.

---

## Documentation

AI was used to draft and refine:

- README.md
- spec.md
- prompts.md
- reflection.md

The documentation was then updated to accurately reflect the final implementation and project structure.

---

## Representative Prompts

Examples of significant prompts used during development include:

- Design a Clean Architecture solution for the Hotel Stay Availability case study using .NET 8, ASP.NET Core Controllers, Blazor WebAssembly, and lightweight CQRS.
- Design an extensible `IHotelProvider` abstraction with deterministic stub implementations and normalized provider responses.
- Implement document validation for domestic and international destinations with appropriate HTTP status codes.
- Build a Blazor WebAssembly user interface for hotel search, reservation, confirmation, and reservation lookup.
- Generate meaningful unit tests covering validation, provider behavior, reservation flow, and lookup scenarios.
- Review the implementation against the original case study requirements and identify any functional or documentation gaps before submission.

---

## Key Design Decisions

The following architectural decisions were made during implementation:

- ASP.NET Core Controllers were chosen instead of Minimal APIs to provide clearer endpoint organization and maintainability.
- Clean Architecture was adopted to separate domain, application, infrastructure, API, UI, and testing concerns.
- Lightweight CQRS was used to separate commands and queries without introducing unnecessary dependencies.
- `IHotelProvider` abstracts hotel providers so additional providers can be added without changing the core search flow.
- A shared destination catalog is reused by both provider implementations and document validation to ensure consistent behavior.
- Provider implementations are deterministic, making testing and demonstrations repeatable.
- Reservations are stored in memory because persistence was outside the scope of the case study.
- Unknown destinations return an empty search result rather than an error, providing predictable search behavior.
- Client-side validation improves the user experience while server-side validation guarantees business rules are enforced for every API consumer.

---

## AI Assisted, Human Reviewed

AI was used to accelerate analysis, implementation, testing, and documentation throughout the project.

All generated code, architectural decisions, documentation, and tests were manually reviewed, refined, executed, and validated against the case study requirements before submission. AI was used as a development assistant rather than a replacement for engineering judgement.
