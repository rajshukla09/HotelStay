# AI Usage Notes

## Initial repository setup

Prompted the AI assistant to create the initial .NET 8 solution structure and documentation for the Hotel Stay Availability case study.

## Constraints captured

- Create documentation first, especially `spec.md`.
- Scaffold a Minimal API project, Blazor WebAssembly project, and xUnit test project.
- Add project references where needed.
- Do not implement business logic yet.
- Ensure the solution builds where the .NET 8 SDK is available.

## Follow-up prompt ideas

- Implement shared domain contracts after reviewing `spec.md`.
- Add request validation for availability searches.
- Add provider abstractions and fake provider implementations.
- Add API integration tests.
- Build the Blazor search form and state handling.
