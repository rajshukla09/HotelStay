using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Domain.Destinations;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Providers;

public sealed class PremierStaysProvider : IHotelProvider
{
    public string ProviderName => "PremierStays";

    public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!DestinationCatalog.IsKnownDestination(request.Destination))
        {
            return Task.FromResult<IReadOnlyCollection<HotelRoomResult>>([]);
        }

        var responses = new[]
        {
            new PremierStaysRoom("ps-standard", $"{request.Destination} Premier Hotel", "standard", 145m, "free-48h", ["WiFi", "Breakfast"], 4),
            new PremierStaysRoom("ps-deluxe", $"{request.Destination} Grand Premier", "deluxe", 215m, "free-48h", ["WiFi", "Breakfast", "Pool"], 5),
            new PremierStaysRoom("ps-suite", $"{request.Destination} Premier Suites", "suite", 330m, "non-refundable", ["WiFi", "Lounge", "Airport shuttle"], 5)
        };

        var results = responses
            .Select(room => Normalize(room, request))
            .Where(room => request.RoomType is null || room.RoomType == request.RoomType)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<HotelRoomResult>>(results);
    }

    private HotelRoomResult Normalize(PremierStaysRoom room, HotelSearchRequest request)
    {
        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var roomType = NormalizeRoomType(room.Category);
        var perNightRate = room.NightlyRate;

        return new HotelRoomResult(
            RoomId: $"{ProviderName}-{request.Destination}-{room.Id}".Replace(' ', '-'),
            Provider: ProviderName,
            HotelName: room.PropertyName,
            Destination: request.Destination,
            RoomType: roomType,
            PerNightRate: perNightRate,
            TotalPrice: perNightRate * nights,
            CancellationPolicy: NormalizeCancellationPolicy(room.CancellationCode),
            Amenities: room.Features,
            StarRating: room.Stars);
    }

    private static RoomType NormalizeRoomType(string category) => category.ToUpperInvariant() switch
    {
        "STANDARD" => RoomType.Standard,
        "DELUXE" => RoomType.Deluxe,
        "SUITE" => RoomType.Suite,
        _ => RoomType.Standard
    };

    private static CancellationPolicy NormalizeCancellationPolicy(string cancellationCode) => cancellationCode.ToUpperInvariant() switch
    {
        "FREE-48H" => CancellationPolicy.FreeCancellation48Hours,
        "NON-REFUNDABLE" => CancellationPolicy.NonRefundable,
        _ => CancellationPolicy.Flexible24Hours
    };

    private sealed record PremierStaysRoom(
        string Id,
        string PropertyName,
        string Category,
        decimal NightlyRate,
        string CancellationCode,
        IReadOnlyList<string> Features,
        int Stars);
}
