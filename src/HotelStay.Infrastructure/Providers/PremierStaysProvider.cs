using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Providers;

public sealed class PremierStaysProvider : IHotelProvider
{
    public string ProviderName => "PremierStays";

    public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var responses = new[]
        {
            new PremierStaysRoom("ps-standard", ProviderName, $"{destination} Premier Hotel", RoomType.Standard, 145m, CancellationPolicy.FreeCancellation48Hours, ["WiFi", "Breakfast"], 4),
            new PremierStaysRoom("ps-deluxe", ProviderName, $"{destination} Grand Premier", RoomType.Deluxe, 215m, CancellationPolicy.FreeCancellation48Hours, ["WiFi", "Breakfast", "Pool"], 5),
            new PremierStaysRoom("ps-suite", ProviderName, $"{destination} Premier Suites", RoomType.Suite, 330m, CancellationPolicy.NonRefundable, ["WiFi", "Lounge", "Airport shuttle"], 5)
        };

        var results = responses
            .Where(room => roomType is null || room.RoomType == roomType)
            .Select(room => new HotelRoomResult(
                RoomId: $"{ProviderName}-{destination}-{room.RoomId}".Replace(' ', '-'),
                Provider: room.ProviderName,
                HotelName: room.HotelName,
                Destination: destination,
                RoomType: room.RoomType,
                PerNightRate: room.PerNightRate,
                TotalPrice: room.PerNightRate * nights,
                CancellationPolicy: room.CancellationPolicy,
                Amenities: room.Amenities,
                StarRating: room.StarRating))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<HotelRoomResult>>(results);
    }

    private sealed record PremierStaysRoom(
        string RoomId,
        string ProviderName,
        string HotelName,
        RoomType RoomType,
        decimal PerNightRate,
        CancellationPolicy CancellationPolicy,
        IReadOnlyList<string> Amenities,
        int StarRating);
}
