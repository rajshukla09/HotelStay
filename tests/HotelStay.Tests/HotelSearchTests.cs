using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Application.Queries;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelStay.Tests;

public sealed class HotelSearchTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly WebApplicationFactory<Program> factory;

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public HotelSearchTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task SearchEndpointReturnsNormalizedResultsSortedByTotalPrice()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/hotels/search?destination=Paris&checkIn=2026-09-01&checkOut=2026-09-04");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<HotelRoomResult>>(JsonOptions);
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Equal(results!.OrderBy(result => result.TotalPrice), results);
        Assert.All(results, result =>
        {
            Assert.Equal("Paris", result.Destination);
            Assert.Equal(result.PerNightRate * 3, result.TotalPrice);
            Assert.False(string.IsNullOrWhiteSpace(result.Provider));
            Assert.False(string.IsNullOrWhiteSpace(result.RoomId));
        });
    }

    [Fact]
    public async Task SearchEndpointFiltersByRoomType()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/hotels/search?destination=Paris&checkIn=2026-09-01&checkOut=2026-09-04&roomType=Deluxe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<HotelRoomResult>>(JsonOptions);
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results!, result => Assert.Equal(RoomType.Deluxe, result.RoomType));
    }

    [Fact]
    public async Task SearchEndpointReturnsEmptyResultsForUnknownDestination()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/hotels/search?destination=Rome&checkIn=2026-09-01&checkOut=2026-09-04");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<HotelRoomResult>>(JsonOptions);
        Assert.NotNull(results);
        Assert.Empty(results!);
    }

    [Fact]
    public async Task ReserveEndpointReturns422ForInternationalNationalIdMismatch()
    {
        using var client = factory.CreateClient();
        var request = new ReservationRequest(
            "room-1",
            "PremierStays",
            "Tokyo",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 4),
            RoomType.Deluxe,
            200m,
            600m,
            "Jordan Guest",
            DocumentType.NationalId,
            "N123456");

        var response = await client.PostAsJsonAsync("/api/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("International destinations require a Passport document.", body);
    }

    [Theory]
    [InlineData("Tokyo", DocumentType.Passport)]
    [InlineData("Sydney", DocumentType.NationalId)]
    public async Task ReserveEndpointAcceptsCatalogDocumentMatches(string destination, DocumentType documentType)
    {
        using var client = factory.CreateClient();
        var request = new ReservationRequest(
            "room-1",
            "PremierStays",
            destination,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 4),
            RoomType.Deluxe,
            200m,
            600m,
            "Jordan Guest",
            documentType,
            "DOC123456");

        var response = await client.PostAsJsonAsync("/api/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task ReservationLookupIncludesCancellationPolicy()
    {
        using var client = factory.CreateClient();
        var request = new ReservationRequest(
            "room-1",
            "PremierStays",
            "Sydney",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 4),
            RoomType.Deluxe,
            200m,
            600m,
            "Jordan Guest",
            DocumentType.NationalId,
            "DOC123456",
            CancellationPolicy.NonRefundable);

        var reserveResponse = await client.PostAsJsonAsync("/api/hotels/reserve", request);
        Assert.Equal(HttpStatusCode.OK, reserveResponse.StatusCode);
        var confirmation = await reserveResponse.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        Assert.NotNull(confirmation);

        var lookupResponse = await client.GetAsync($"/api/hotels/reservation/{confirmation!.Reference}");

        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);
        var details = await lookupResponse.Content.ReadFromJsonAsync<ReservationDetails>(JsonOptions);
        Assert.NotNull(details);
        Assert.Equal(request.CancellationPolicy, details!.CancellationPolicy);
    }

    [Theory]
    [InlineData("/api/hotels/search?checkIn=2026-09-01&checkOut=2026-09-04", "Destination is required.")]
    [InlineData("/api/hotels/search?destination=Paris&checkOut=2026-09-04", "Check-in date is required.")]
    [InlineData("/api/hotels/search?destination=Paris&checkIn=2026-09-01", "Check-out date is required.")]
    [InlineData("/api/hotels/search?destination=Paris&checkIn=2026-09-04&checkOut=2026-09-01", "Check-out date must be after check-in date.")]
    public async Task SearchEndpointValidatesRequiredQueryParameters(string url, string expectedMessage)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedMessage, body);
    }

    [Fact]
    public async Task SwaggerIncludesHotelSearchEndpoint()
    {
        using var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();

        var swagger = await client.GetStringAsync("/swagger/v1/swagger.json");

        Assert.Contains("/api/Hotels/search", swagger);
        Assert.Contains("destination", swagger);
        Assert.Contains("checkIn", swagger);
        Assert.Contains("checkOut", swagger);
    }

    [Fact]
    public async Task HandlerCalculatesTotalStayAndSortsProviderResults()
    {
        var handler = new SearchHotelsQueryHandler(new IHotelProvider[]
        {
            new StubHotelProvider("Expensive", 200m),
            new StubHotelProvider("Affordable", 100m)
        });

        var result = await handler.HandleAsync(
            new SearchHotelsQuery("Rome", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), RoomType.Standard),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(new[] { 400m, 800m }, result.Value!.Select(room => room.TotalPrice).ToArray());
    }

    private sealed class StubHotelProvider : IHotelProvider
    {
        private readonly decimal nightlyRate;

        public StubHotelProvider(string providerName, decimal nightlyRate)
        {
            ProviderName = providerName;
            this.nightlyRate = nightlyRate;
        }

        public string ProviderName { get; }

        public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken)
        {
            var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
            IReadOnlyCollection<HotelRoomResult> results =
            [
                new HotelRoomResult(
                    $"{ProviderName}-standard",
                    ProviderName,
                    $"{request.Destination} {ProviderName}",
                    request.Destination,
                    RoomType.Standard,
                    nightlyRate,
                    nightlyRate * nights,
                    CancellationPolicy.Flexible24Hours,
                    null,
                    null)
            ];

            return Task.FromResult(results);
        }
    }
}
