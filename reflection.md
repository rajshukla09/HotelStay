# Reflection

## Overview

The completed Hotel Stay Availability solution is a .NET 8 case study built with a Clean Architecture structure. The backend uses ASP.NET Core Controllers for the HTTP API and a lightweight CQRS approach to keep search queries and reservation commands easy to reason about. The frontend is a Blazor WebAssembly application that supports the main search and reservation flows.

The implementation uses two deterministic hotel provider stubs instead of real external integrations. Search results are normalized through the application layer, while reservations are stored in memory. This keeps the solution focused on the requested behavior: searching supported destinations, validating the correct travel document type, creating reservations, and looking reservations up without introducing infrastructure that was outside the case study scope.

## Design Decisions

Clean Architecture was used to separate responsibilities and keep the solution understandable. Domain concepts remain independent from infrastructure details, application handlers coordinate use cases, infrastructure owns provider and storage implementations, and the API project focuses on HTTP concerns. This made it easier to keep business rules from leaking into controllers or provider stubs.

ASP.NET Core Controllers were chosen instead of Minimal APIs because the API has multiple hotel-related operations and validation outcomes. Controllers provide a familiar structure for grouping endpoints, returning specific HTTP responses, and keeping the code maintainable as the project grows.

A lightweight CQRS style was used to separate reads and writes without adding unnecessary framework complexity. Hotel search is naturally a query, while creating a reservation is a command that performs validation and changes application state. Keeping these paths separate made each handler smaller and easier to test.

The provider abstraction through `IHotelProvider` was an important extensibility point. Both deterministic provider stubs can be treated consistently by the application layer, and a third provider could be added by implementing the same interface. This also keeps provider-specific result generation away from the API and reservation logic.

An in-memory reservation store was selected because durable persistence was outside the scope of the case study. It supports the required reservation flow and lookup behavior while avoiding database setup, migrations, connection strings, and deployment concerns that would distract from the core requirements.

A shared destination catalog was used so provider behavior and document validation rely on the same understanding of supported destinations. This reduces the chance of inconsistencies, such as a provider returning rooms for a destination that validation treats differently, or a domestic destination being validated as international.

## Trade-offs

Several trade-offs were made to keep the solution appropriate for a focused case study. Reservations are stored in memory instead of a database, so data is lost when the API restarts. That is acceptable for demonstrating the flow, but a production system would need durable persistence and stronger concurrency guarantees.

The hotel providers are deterministic stubs rather than real integrations. This makes the application predictable and easy to test, but it does not cover real-world provider concerns such as authentication, rate limits, network failures, retries, response mapping differences, or provider-specific availability rules.

Pricing is intentionally simple and represented as decimal values. This is enough to display and reserve rooms in the case study, but a production booking system would need currency handling, taxes, fees, rounding rules, discounts, and potentially provider-specific price guarantees.

Authentication and authorization were not implemented. This keeps the sample easy to run locally, but production reservation endpoints would need identity, authorization policies, abuse protection, and auditability.

Caching and background processing were also left out. For the current deterministic providers, caching would add complexity without much value. Real provider integrations may benefit from caching search responses, background refreshes, queue-based booking workflows, or asynchronous reconciliation jobs.

Overall, the solution favors clarity and maintainability over enterprise complexity. The structure leaves room for production features, but it avoids adding infrastructure before the requirements justify it.

## What I Would Improve With More Time

With more time, the first improvement would be database persistence, likely using EF Core with SQL Server, so reservations survive restarts and can be queried reliably. That would also introduce migrations, indexes, better concurrency handling, and a clearer persistence model.

The provider stubs could be replaced or supplemented with real hotel provider integrations. That would require configuration-driven credentials, resilient HTTP clients, provider-specific mapping, retry policies, timeout handling, and better error reporting when providers are unavailable.

Authentication and authorization would be important before exposing reservation functionality beyond a local demo. I would also add better search filtering, pagination, and sorting so users can refine results by price, provider, room type, or availability.

Mapping is currently simple enough to keep explicit. If the DTO mapping grew, AutoMapper or Mapster could become useful, but I would only introduce one after the mapping complexity justified the dependency.

Testing could also be expanded. The solution would benefit from more comprehensive integration tests around full API flows, document mismatch responses, reservation lookup behavior, and provider aggregation. Logging and monitoring could be improved with structured logs, request correlation, health checks for downstream providers, and metrics.

Operationally, I would add Docker support and a CI/CD pipeline that restores, builds, tests, and packages the solution consistently. The destination catalog could become configuration-driven or database-backed so new destinations do not require code changes. Finally, the Blazor UI could be improved with localization, stronger accessibility checks, and a more polished responsive/mobile experience.

## Lessons Learned

Designing the provider abstraction first simplified normalization because the application layer could work with a consistent provider contract instead of handling each provider directly. That made it easier to add deterministic providers and kept the API independent of provider-specific details.

Keeping validation in dedicated services improved maintainability. Document rules are central to the reservation flow, and isolating them made the behavior easier to understand and test than embedding the checks directly in controllers.

Clean Architecture made the project responsibilities easier to understand. The separation between domain, application, infrastructure, API, and UI helped keep each project focused and made it clearer where future changes should be made.

Client-side validation improves the user experience by catching obvious issues earlier, while server-side validation protects the business rules regardless of which client calls the API. Both are useful, but the server-side rules remain the source of truth.

The deterministic provider stubs made testing simpler because search behavior is repeatable. This was especially useful for a case study, where reviewers should be able to run the same commands and see consistent results.

## Conclusion

The solution was intentionally designed to satisfy the case study requirements while remaining clean, extensible, testable, and easy to evolve. It demonstrates the core hotel search and reservation workflows without unnecessary infrastructure, while leaving clear extension points for persistence, real provider integrations, authentication, richer search capabilities, and production operations.
