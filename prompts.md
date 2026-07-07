# AI Usage Notes

## Overview

AI assistance was used throughout the software development lifecycle for analysis, architecture, design, implementation, testing, and documentation. The prompts focused on turning the case study requirements into a small, reviewable .NET 8 solution with a clear backend, frontend, validation flow, and documentation. The implementation, verification steps, and design decisions were reviewed manually before submission.

## Analysis

AI was used to help interpret the Hotel Stay Availability case study, identify the main functional requirements, and organize the work into manageable implementation steps. The analysis prompts focused on search availability, supported destinations, reservation creation, reservation lookup, document rules for domestic and international travel, expected HTTP outcomes, and how to keep the scope appropriate for a case study.

## Architecture

Architecture prompts were used to plan a Clean Architecture structure with separate Domain, Application, Infrastructure, API, Blazor, and test projects. The design discussion included using ASP.NET Core Controllers instead of Minimal APIs for better organization, a lightweight CQRS style for separating search queries from reservation commands, SOLID principles, dependency injection, provider abstraction through `IHotelProvider`, an in-memory reservation store, and a shared destination catalog used by both providers and validation.

## Backend Implementation

Backend implementation prompts supported the hotel search flow, provider result normalization, reservation creation, reservation lookup, document validation, and consistent error handling. AI was also used to assist with Swagger setup and FluentValidation rules so the API behavior was easier to exercise and validate during review.

## Blazor UI

Blazor UI prompts supported building the main search page, displaying hotel results, collecting reservation details, showing a confirmation screen, and keeping API calls behind a client abstraction. The UI prompts also covered client-side validation and using configuration for the API base endpoint so the frontend can target the local API cleanly.

## Testing

Testing prompts were used to identify useful unit and integration test coverage, including validation scenarios, edge cases, Swagger-based API testing, and end-to-end checks of the search and reservation flows. Particular attention was given to document mismatch cases, deterministic provider behavior, unknown destinations returning no rooms, and reservation lookup behavior.

## Documentation

Documentation prompts were used to create and refine `README.md`, `spec.md`, `reflection.md`, and `prompts.md`. The documentation work focused on making the project easy to review by capturing requirements, run instructions, architecture decisions, assumptions, API examples, improvement areas, and a transparent summary of AI usage.

## Key Design Decisions

- Controllers were chosen instead of Minimal APIs for maintainability and clearer endpoint organization.
- Clean Architecture was used to separate domain rules, application use cases, infrastructure implementations, API hosting, and UI concerns.
- Lightweight CQRS was used instead of introducing MediatR, keeping reads and writes separate without adding unnecessary dependencies.
- A shared destination catalog was reused by providers and validation to keep supported destination behavior consistent.
- Deterministic provider stubs were used so search results are repeatable and tests are easier to reason about.
- An in-memory reservation store was used because durable persistence was outside the case study scope.
- Unknown destinations return no rooms rather than throwing an error, which keeps search behavior simple and predictable.
- Client-side validation improves the user experience, while server-side validation enforces the business rules for every API caller.

## AI Assisted But Human Reviewed

AI was used to accelerate analysis, implementation, testing, and documentation. All generated code, architecture, and design decisions were manually reviewed, refined, tested, and validated against the case study requirements before submission.
