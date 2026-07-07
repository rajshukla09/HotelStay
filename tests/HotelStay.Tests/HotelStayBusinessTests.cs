using HotelStay.Application.Commands;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Application.Queries;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers;
using HotelStay.Infrastructure.Services;
using HotelStay.Infrastructure.Stores;

namespace HotelStay.Tests;

public sealed class HotelStayBusinessTests
{
    [Theory]
    [InlineData(null, "2026-09-01", "2026-09-04", "Destination is required.")]
    [InlineData("Paris", null, "2026-09-04", "Check-in date is required.")]
    [InlineData("Paris", "2026-09-01", null, "Check-out date is required.")]
    [InlineData("Paris", "2026-09-04", "2026-09-04", "Check-out date must be after check-in date.")]
    [InlineData("Paris", "2026-09-05", "2026-09-04", "Check-out date must be after check-in date.")]
    public async Task SearchValidation_ReturnsValidationFailure_ForInvalidRequests(string? destination, string? checkIn, string? checkOut, string expectedMessage)
    {
        // Arrange
        var handler = new SearchHotelsQueryHandler([new PremierStaysProvider()]);
        var query = new SearchHotelsQuery(destination, ParseDate(checkIn), ParseDate(checkOut), null);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
    }

    [Fact]
    public async Task SearchValidation_Passes_ForValidRequest()
    {
        // Arrange
        var handler = new SearchHotelsQueryHandler([new PremierStaysProvider()]);
        var query = new SearchHotelsQuery("Paris", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 4), null);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value!);
    }

    [Fact]
    public async Task PremierStays_ReturnsAvailableRooms_WithAmenitiesAndStarRatings_Deterministically()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = SearchRequest("Sydney");

        // Act
        var first = await provider.SearchAsync(request, CancellationToken.None);
        var second = await provider.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotEmpty(first);
        Assert.Equal(
            first.Select(room => (room.RoomId, room.Provider, room.HotelName, room.RoomType, room.PerNightRate, room.TotalPrice, Amenities: string.Join(",", room.Amenities ?? []), room.StarRating)),
            second.Select(room => (room.RoomId, room.Provider, room.HotelName, room.RoomType, room.PerNightRate, room.TotalPrice, Amenities: string.Join(",", room.Amenities ?? []), room.StarRating)));
        Assert.All(first, room =>
        {
            Assert.Equal("PremierStays", room.Provider);
            Assert.NotNull(room.Amenities);
            Assert.NotEmpty(room.Amenities!);
            Assert.True(room.StarRating is >= 4 and <= 5);
            Assert.True(room.TotalPrice > 0);
        });
    }

    [Fact]
    public async Task BudgetNests_FiltersUnavailableRooms_AndSupportsAllRoomTypes_Deterministically()
    {
        // Arrange
        var provider = new BudgetNestsProvider();
        var parisRequest = SearchRequest("Paris");
        var sydneyRequest = SearchRequest("Sydney");

        // Act
        var parisResults = await provider.SearchAsync(parisRequest, CancellationToken.None);
        var firstSydneyResults = await provider.SearchAsync(sydneyRequest, CancellationToken.None);
        var secondSydneyResults = await provider.SearchAsync(sydneyRequest, CancellationToken.None);

        // Assert
        Assert.Equal(firstSydneyResults, secondSydneyResults);
        Assert.DoesNotContain(parisResults, room => room.RoomType == RoomType.Suite);
        Assert.Contains(firstSydneyResults, room => room.RoomType == RoomType.Standard);
        Assert.Contains(firstSydneyResults, room => room.RoomType == RoomType.Deluxe);
        Assert.Contains(firstSydneyResults, room => room.RoomType == RoomType.Suite);
    }

    [Fact]
    public async Task SearchHandler_QueriesBothProviders_NormalizesTotals_FiltersRoomType_AndSortsByTotalPrice()
    {
        // Arrange
        var providers = new CountingProvider[]
        {
            new("Premium", 250m, RoomType.Deluxe),
            new("Value", 100m, RoomType.Deluxe),
            new("FilteredOut", 10m, RoomType.Standard)
        };
        var handler = new SearchHotelsQueryHandler(providers);
        var query = new SearchHotelsQuery("Tokyo", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 4), RoomType.Deluxe);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.All(providers, provider => Assert.Equal(1, provider.CallCount));
        var rooms = Assert.IsAssignableFrom<IReadOnlyList<HotelRoomResult>>(result.Value);
        Assert.Equal([300m, 750m], rooms.Select(room => room.TotalPrice).ToArray());
        Assert.All(rooms, room =>
        {
            Assert.Equal(RoomType.Deluxe, room.RoomType);
            Assert.Equal(room.PerNightRate * 3, room.TotalPrice);
            Assert.Equal("Tokyo", room.Destination);
        });
    }

    [Theory]
    [InlineData("Sydney", DestinationCategory.Domestic)]
    [InlineData("Melbourne", DestinationCategory.Domestic)]
    [InlineData("London", DestinationCategory.International)]
    [InlineData("Paris", DestinationCategory.International)]
    [InlineData("Tokyo", DestinationCategory.International)]
    public void DocumentValidation_ClassifiesDestinations(string destination, DestinationCategory expectedCategory)
    {
        // Arrange
        var service = new DocumentValidationService();

        // Act
        var result = service.Validate(destination, DocumentType.Passport, "P123456");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(expectedCategory, result.Value);
    }

    [Fact]
    public void DocumentValidation_AcceptsNationalIdForDomestic_AndRejectsItForInternationalWithClearMessage()
    {
        // Arrange
        var service = new DocumentValidationService();

        // Act
        var domestic = service.Validate("Sydney", DocumentType.NationalId, "N123456");
        var international = service.Validate("London", DocumentType.NationalId, "N123456");

        // Assert
        Assert.True(domestic.Succeeded);
        Assert.Equal(DestinationCategory.Domestic, domestic.Value);
        Assert.False(international.Succeeded);
        Assert.Equal(422, international.StatusCode);
        Assert.Equal("International destinations require a Passport document.", international.ErrorMessage);
    }

    [Fact]
    public async Task ReservationHandler_CreatesAndStoresReservation_WithReferenceTotalPriceAndCancellationPolicy()
    {
        // Arrange
        var store = new InMemoryReservationStore();
        var handler = new ReserveHotelCommandHandler(new DocumentValidationService(), store);
        var request = ReservationRequest(destination: "Sydney", documentType: DocumentType.NationalId);

        // Act
        var result = await handler.HandleAsync(new ReserveHotelCommand(request), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.StartsWith("HST-", result.Value!.Reference);
        Assert.Equal(request.TotalPrice, result.Value.TotalPrice);
        Assert.Equal(request.CancellationPolicy, result.Value.CancellationPolicy);
        var stored = await store.GetByReferenceAsync(result.Value.Reference, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal(request.GuestName, stored!.GuestName);
        Assert.Equal(request.TotalPrice, stored.TotalPrice);
    }

    [Fact]
    public async Task ReservationHandler_RejectsInvalidDocumentType_With422ValidationResult()
    {
        // Arrange
        var store = new InMemoryReservationStore();
        var handler = new ReserveHotelCommandHandler(new DocumentValidationService(), store);
        var request = ReservationRequest(destination: "Tokyo", documentType: DocumentType.NationalId);

        // Act
        var result = await handler.HandleAsync(new ReserveHotelCommand(request), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("International destinations require a Passport document.", result.ErrorMessage);
    }

    [Fact]
    public async Task ReservationLookup_ReturnsReservationDetails_ForExistingReference_AndNotFoundForMissingReference()
    {
        // Arrange
        var store = new InMemoryReservationStore();
        var reservation = new ReservationDetails(
            "HST-TEST-123456", "room-1", "PremierStays", "Melbourne", DestinationCategory.Domestic,
            new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 3), RoomType.Standard, 120m, 240m,
            "Casey Guest", DocumentType.NationalId, "N123", DateTimeOffset.UtcNow);
        await store.SaveAsync(reservation, CancellationToken.None);
        var handler = new GetReservationByReferenceQueryHandler(store);

        // Act
        var existing = await handler.HandleAsync(new GetReservationByReferenceQuery("HST-TEST-123456"), CancellationToken.None);
        var missing = await handler.HandleAsync(new GetReservationByReferenceQuery("missing"), CancellationToken.None);

        // Assert
        Assert.True(existing.Succeeded);
        Assert.Equal(reservation, existing.Value);
        Assert.False(missing.Succeeded);
        Assert.Equal(404, missing.StatusCode);
        Assert.Equal("Reservation not found.", missing.ErrorMessage);
    }

    private static HotelSearchRequest SearchRequest(string destination, RoomType? roomType = null) =>
        new(destination, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 4), roomType);

    private static DateOnly? ParseDate(string? value) => value is null ? null : DateOnly.Parse(value);

    private static ReservationRequest ReservationRequest(string destination, DocumentType documentType) =>
        new("room-1", "PremierStays", destination, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 4),
            RoomType.Deluxe, 200m, 600m, "Jordan Guest", documentType, "DOC123");

    private sealed class CountingProvider : IHotelProvider
    {
        private readonly decimal perNightRate;
        private readonly RoomType roomType;

        public CountingProvider(string providerName, decimal perNightRate, RoomType roomType)
        {
            ProviderName = providerName;
            this.perNightRate = perNightRate;
            this.roomType = roomType;
        }

        public string ProviderName { get; }
        public int CallCount { get; private set; }

        public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
            IReadOnlyCollection<HotelRoomResult> result =
            [
                new HotelRoomResult(
                    $"{ProviderName}-room", ProviderName, $"{request.Destination} Hotel", request.Destination,
                    roomType, perNightRate, perNightRate * nights, CancellationPolicy.Flexible24Hours, null, null)
            ];
            return Task.FromResult(result);
        }
    }
}
