# Hotel Stay Availability Specification

## Problem summary

Hotel Stay Availability is a .NET 8 case study application for searching hotel rooms by destination and stay dates, reserving a selected room, and looking up reservations by reference. The implemented solution includes an ASP.NET Core backend, a Blazor WebAssembly frontend, deterministic provider stubs, document validation rules, and an in-memory reservation store.

## Architecture

The solution uses Clean Architecture with separate projects for domain concepts, application use cases, infrastructure implementations, API hosting, Blazor UI, and tests.

- `HotelStay.Domain` contains shared domain enums, reservation details, and the supported destination catalog.
- `HotelStay.Application` contains DTOs, lightweight CQRS-style commands and queries, handlers, application interfaces, and operation result types.
- `HotelStay.Infrastructure` contains deterministic hotel providers, document validation, in-memory reservation storage, and dependency injection registration.
- `HotelStay.Api` hosts ASP.NET Core Controllers, Swagger, CORS, global exception middleware, and application handler registration.
- `HotelStay.Blazor` implements the guest-facing Blazor WebAssembly flow.
- `HotelStay.Tests` contains API and business behavior tests.

The backend uses Controllers rather than Minimal APIs. Search, reservation creation, and reservation lookup are grouped under `HotelsController`, while health is exposed by `HealthController`. The application layer uses lightweight CQRS naming and handler classes without MediatR: `SearchHotelsQueryHandler`, `ReserveHotelCommandHandler`, and `GetReservationByReferenceQueryHandler`.

## Solution structure

```text
HotelStay.sln
├── src/
│   ├── HotelStay.Api/
│   ├── HotelStay.Application/
│   ├── HotelStay.Domain/
│   ├── HotelStay.Infrastructure/
│   └── HotelStay.Blazor/
└── tests/
    └── HotelStay.Tests/
```

## Supported destinations

The implemented destination catalog is shared by providers, backend validation, and the Blazor UI.

Domestic destinations:

- Sydney
- Melbourne

International destinations:

- London
- Paris
- Tokyo

Unknown destinations are treated differently by operation:

- Hotel search returns `200 OK` with an empty result list because providers return no rooms for unknown destinations.
- Reservation validation returns `422 Unprocessable Entity` because reservations require a supported destination category.

## Domain models

Implemented domain enums:

- `DestinationCategory`: `Domestic`, `International`
- `DocumentType`: `Passport`, `NationalId`
- `RoomType`: `Standard`, `Deluxe`, `Suite`
- `CancellationPolicy`: `FreeCancellation48Hours`, `Flexible24Hours`, `NonRefundable`

Implemented destination catalog:

- `DestinationCatalog.Destinations`
- `DestinationCatalog.DomesticDestinations`
- `DestinationCatalog.InternationalDestinations`
- `DestinationCatalog.TryGetCategory(...)`
- `DestinationCatalog.IsKnownDestination(...)`

Implemented reservation entity:

```csharp
public sealed record ReservationDetails(
    string Reference,
    string RoomId,
    string Provider,
    string Destination,
    DestinationCategory DestinationCategory,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    DateTimeOffset CreatedAt);
```

## Application models

Implemented API/application DTOs. The API configures JSON enum string conversion, so request and response examples use enum names such as `Standard`, `NationalId`, and `FreeCancellation48Hours`:

```csharp
public sealed record HotelSearchRequest(
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType? RoomType);

public sealed record HotelRoomResult(
    string RoomId,
    string Provider,
    string HotelName,
    string Destination,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    IReadOnlyList<string>? Amenities,
    int? StarRating);

public sealed record ReservationRequest(
    string RoomId,
    string Provider,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    CancellationPolicy CancellationPolicy = CancellationPolicy.Flexible24Hours);

public sealed record ReservationResponse(
    string Reference,
    string Message,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy);
```

## API contracts

### `GET /health`

Returns service health for local development and smoke testing.

Successful response:

```json
{
  "status": "Healthy"
}
```

### `GET /api/hotels/search`

Searches deterministic providers for normalized hotel room results.

Query parameters:

- `destination` (`string`, required)
- `checkIn` (`DateOnly`, required)
- `checkOut` (`DateOnly`, required)
- `roomType` (`RoomType`, optional: `Standard`, `Deluxe`, `Suite`)

Example:

```http
GET /api/hotels/search?destination=Sydney&checkIn=2026-07-08&checkOut=2026-07-10
```

Successful response: `200 OK` with a JSON array of `HotelRoomResult` objects sorted by `TotalPrice` ascending.

Validation response: `400 Bad Request` when required query parameters are missing or `checkOut` is not after `checkIn`.

Error body shape:

```json
{
  "error": "Error",
  "message": "Check-out date must be after check-in date."
}
```

### `POST /api/hotels/reserve`

Creates an in-memory reservation after document validation succeeds.

Request body shape:

```json
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

Successful response: `200 OK` with a `ReservationResponse` containing the generated reservation reference, message, total price, and cancellation policy.

Validation responses:

- `400 Bad Request` when `roomId` or `provider` is missing.
- `400 Bad Request` when `guestName` is missing.
- `400 Bad Request` when `documentNumber` is missing.
- `400 Bad Request` when `checkOut` is not after `checkIn`.
- `400 Bad Request` when supplied pricing is not greater than zero.
- `422 Unprocessable Entity` when the destination is unsupported.
- `422 Unprocessable Entity` when an international destination is reserved with `NationalId` instead of `Passport`.

### `GET /api/hotels/reservation/{reference}`

Looks up an in-memory reservation by reference.

Successful response: `200 OK` with `ReservationDetails`.

Missing reservation response: `404 Not Found` with message `Reservation not found.`

## Provider interface

Implemented provider abstraction:

```csharp
public interface IHotelProvider
{
    string ProviderName { get; }

    Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(
        HotelSearchRequest request,
        CancellationToken cancellationToken);
}
```

Provider implementations:

- `PremierStaysProvider`
  - Returns standard, deluxe, and suite rooms for known destinations.
  - Includes amenities and star ratings.
  - Normalizes provider-specific room categories and cancellation codes.
- `BudgetNestsProvider`
  - Returns available budget rooms for known destinations.
  - Does not include amenities or star ratings.
  - Filters out unavailable rooms; the Paris suite is unavailable.
  - Normalizes numeric room type and cancellation tier values.

Both providers:

- Return an empty collection for unknown destinations.
- Compute `TotalPrice` as `PerNightRate * night count`.
- Respect cancellation tokens.
- Return deterministic data for repeatable tests and review.

## Validation rules

Search validation:

- `destination` is required.
- `checkIn` is required.
- `checkOut` is required.
- `checkOut` must be after `checkIn`.
- Optional `roomType` filters results to matching room types.
- Search does not reject unknown destinations; unknown destinations return no rooms.

Reservation validation:

- `roomId` is required.
- `provider` is required.
- `guestName` is required.
- `documentNumber` is required.
- `checkOut` must be after `checkIn`.
- `perNightRate` and `totalPrice` must be greater than zero.
- `destination` must exist in the shared destination catalog.
- The document type must satisfy the destination category rules below.

Document validation:

- Domestic destinations accept `NationalId` and `Passport`.
- International destinations require `Passport`.
- International destinations with `NationalId` return `422 Unprocessable Entity` and message `International destinations require a Passport document.`
- Unsupported reservation destinations return `422 Unprocessable Entity` and message `Destination is not supported.`

## Reservation storage

Reservations are stored in `InMemoryReservationStore`, which uses a case-insensitive `ConcurrentDictionary<string, ReservationDetails>` keyed by reservation reference.

The generated reservation reference uses the format:

```text
HST-{yyyyMMdd}-{six-character uppercase hexadecimal hash}
```

The store is registered as a singleton. Reservations are not persisted across API process restarts.

## Frontend states

The Blazor WebAssembly UI implements the following states and flows:

- Initial hotel search form at `/hotels`.
- Destination dropdown populated from `DestinationCatalog`.
- Destination category hints for domestic and international destinations.
- Client-side search validation for required destination and dates, supported destinations, and date ordering.
- Loading state while search is in progress.
- Results state with normalized provider room cards.
- Empty-results state when no rooms are returned.
- Sort order toggle for total price ascending or descending.
- Reservation form for the selected room.
- Client-side reservation validation for guest name, document type, document number, and international passport requirement.
- Confirmation state after successful reservation.
- Reservation lookup by reference.
- Lookup not-found state when the API returns `404`.
- API error message state for failed search or reservation calls.

The frontend uses `IHotelApiService` and `IApiClient` abstractions. The API base URL is configuration-driven through Blazor app settings.

## Testing strategy

Implemented tests cover both API and business behavior:

- Health endpoint smoke test.
- Hotel search endpoint returns normalized provider results sorted by total price.
- Room type filtering.
- Unknown search destinations return an empty result set.
- Search query validation failures return `400 Bad Request`.
- Swagger includes the hotel search endpoint in Development and schema examples for room results, reservation requests, reservation responses, and reservation details.
- Provider determinism for `PremierStaysProvider` and `BudgetNestsProvider`.
- Provider handling of unavailable rooms.
- Destination catalog domestic and international city definitions.
- Document validation for domestic and international destinations.
- Unsupported reservation destination validation with `422`.
- International `NationalId` mismatch validation with `422`.
- Reservation creation stores reference, total price, guest details, and cancellation policy.
- Reservation lookup returns details for an existing reference and `404` for a missing reference.

## Extensibility notes

To add a third hotel provider:

- Implement `IHotelProvider` in the Infrastructure project or a provider-specific infrastructure assembly.
- Keep provider-specific DTOs, mapping, availability rules, and error handling isolated inside the provider implementation.
- Normalize results to `HotelRoomResult` before returning to the application layer.
- Register the provider with dependency injection as another `IHotelProvider` implementation.
- Add provider-specific tests for deterministic behavior, mapping, cancellation policy conversion, total price calculation, and unknown destination handling.
- Confirm existing API response contracts remain unchanged.

Potential production extensions:

- Replace `InMemoryReservationStore` with durable database persistence.
- Move the destination catalog to configuration or persistent storage.
- Add authentication and authorization for reservation operations.
- Add real provider integrations with resilient HTTP clients, retries, timeouts, and provider health checks.
- Add pagination, richer filtering, and currency handling if provider result volumes or pricing complexity increase.
