# Hotel Stay Availability Specification

## Problem summary

Build a Hotel Stay Availability application that allows a guest-facing frontend to request hotel availability for a destination and date range, then display normalized availability results returned by external hotel availability providers.

This initial task establishes the repository, solution, project structure, and documentation only. Business logic, provider integrations, persistence, and production-ready validation are intentionally deferred.

## Assumptions

- The system targets .NET 8.
- The backend is a Minimal API that will eventually orchestrate availability searches across providers.
- The frontend is a Blazor WebAssembly application that will call the backend API.
- The first implementation will support two providers, with the design prepared for adding a third provider later.
- Prices are represented in a single currency per result unless explicitly expanded later.
- Dates are interpreted as local hotel stay dates, not instants in UTC.
- Availability searches are read-only and do not reserve inventory.
- Authentication, authorization, payment, booking, and user profiles are out of scope for the initial case study.

## Domain models

Planned domain concepts:

- `AvailabilitySearchRequest`
  - Destination or hotel/location identifier.
  - Check-in date.
  - Check-out date.
  - Guest count.
  - Room count.
- `AvailabilitySearchResult`
  - Provider identifier.
  - Hotel identifier.
  - Hotel name.
  - Room type identifier.
  - Room type name.
  - Availability status.
  - Nightly price.
  - Total price.
  - Currency code.
  - Cancellation policy summary.
- `ProviderAvailabilityResult`
  - Provider-specific normalized result returned to the orchestration layer.
- `Money`
  - Amount.
  - ISO 4217 currency code.
- `StayDateRange`
  - Check-in date.
  - Check-out date.
  - Computed night count.

These models are documented for planning only and are not implemented in this task.

## API contracts

Planned API endpoints:

### `GET /health`

Returns service health for local development and smoke testing.

### `POST /api/availability/search`

Searches for hotel stay availability.

Request shape, planned:

```json
{
  "destination": "NYC",
  "checkInDate": "2026-09-01",
  "checkOutDate": "2026-09-04",
  "guests": 2,
  "rooms": 1
}
```

Response shape, planned:

```json
{
  "results": [
    {
      "provider": "ProviderA",
      "hotelId": "hotel-123",
      "hotelName": "Example Hotel",
      "roomTypeId": "king-standard",
      "roomTypeName": "Standard King",
      "available": true,
      "nightlyPrice": 199.99,
      "totalPrice": 599.97,
      "currency": "USD",
      "cancellationPolicy": "Free cancellation before the configured cutoff."
    }
  ]
}
```

Error response shape, planned:

```json
{
  "error": "ValidationFailed",
  "details": ["Check-out date must be after check-in date."]
}
```

The availability endpoint is not implemented in this task.

## Provider interface contract

Planned provider abstraction:

```csharp
public interface IAvailabilityProvider
{
    string ProviderName { get; }

    Task<IReadOnlyCollection<ProviderAvailabilityResult>> SearchAsync(
        AvailabilitySearchRequest request,
        CancellationToken cancellationToken);
}
```

Provider implementations should:

- Accept a normalized search request.
- Return normalized availability results.
- Respect cancellation tokens.
- Isolate provider-specific DTOs, mapping, authentication, and error handling from the API layer.
- Avoid leaking provider-specific response shapes into frontend contracts.

## Validation rules

Planned validation rules:

- Destination is required.
- Check-in date is required.
- Check-out date is required.
- Check-out date must be after check-in date.
- Guest count must be at least 1.
- Room count must be at least 1.
- Guest and room counts must stay within configured maximums.
- Date ranges may be limited by a configured maximum stay length.
- Searches in the past may be rejected.
- Currency codes must be valid ISO 4217 codes when supplied.

## Frontend states

Planned Blazor UI states:

- Initial empty search form.
- Client-side validation errors.
- Loading/search in progress.
- Results available.
- No availability found.
- Backend validation failure.
- Provider partial failure with still-displayable results.
- Unexpected error.
- Retry-ready state after a failed search.

## Test strategy

Planned test coverage:

- Unit tests for validation rules.
- Unit tests for provider response mapping.
- Unit tests for provider orchestration and result normalization.
- API tests for request/response contracts and error handling.
- Frontend component tests for form state transitions, if component testing is added.
- Smoke test that the API starts and exposes health checks.
- Regression tests for adding future providers without changing API contracts.

This task only creates the xUnit project and verifies the solution can build where the .NET SDK is available.

## Extensibility notes for adding a third provider

To add a third provider later:

- Implement the shared `IAvailabilityProvider` contract in a provider-specific class.
- Keep provider DTOs and mapping logic isolated in the provider implementation area.
- Register the implementation with dependency injection without changing existing providers.
- Add provider-specific configuration through options binding.
- Add contract and mapping tests for the new provider.
- Confirm API response contracts remain stable for the frontend.
- Ensure provider failures can be handled independently so one provider outage does not necessarily fail the entire search.
