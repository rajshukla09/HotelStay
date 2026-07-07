using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Providers;

public sealed class BudgetNestsProvider : IHotelProvider
{
    public string ProviderName => "BudgetNests";

    public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var responses = new[]
        {
            new BudgetNestsRoom("bn-standard", $"{request.Destination} Budget Nest", 0, 95m, 1, true),
            new BudgetNestsRoom("bn-deluxe", $"{request.Destination} City Nest", 1, 155m, 2, IsAvailable(request.Destination, RoomType.Deluxe)),
            new BudgetNestsRoom("bn-suite", $"{request.Destination} Family Nest", 2, 240m, 1, IsAvailable(request.Destination, RoomType.Suite))
        };

        var results = responses
            .Where(room => room.available)
            .Select(room => Normalize(room, request))
            .Where(room => request.RoomType is null || room.RoomType == request.RoomType)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<HotelRoomResult>>(results);
    }

    private HotelRoomResult Normalize(BudgetNestsRoom room, HotelSearchRequest request)
    {
        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var perNightRate = room.per_night_rate;

        return new HotelRoomResult(
            RoomId: $"{ProviderName}-{request.Destination}-{room.room_id}".Replace(' ', '-'),
            Provider: ProviderName,
            HotelName: room.hotel_name,
            Destination: request.Destination,
            RoomType: NormalizeRoomType(room.room_type_code),
            PerNightRate: perNightRate,
            TotalPrice: perNightRate * nights,
            CancellationPolicy: NormalizeCancellationPolicy(room.cancellation_tier),
            Amenities: null,
            StarRating: null);
    }

    private static RoomType NormalizeRoomType(int roomTypeCode) => roomTypeCode switch
    {
        0 => RoomType.Standard,
        1 => RoomType.Deluxe,
        2 => RoomType.Suite,
        _ => RoomType.Standard
    };

    private static CancellationPolicy NormalizeCancellationPolicy(int cancellationTier) => cancellationTier switch
    {
        1 => CancellationPolicy.Flexible24Hours,
        2 => CancellationPolicy.NonRefundable,
        _ => CancellationPolicy.FreeCancellation48Hours
    };

    private static bool IsAvailable(string destination, RoomType roomType)
    {
        if (roomType == RoomType.Suite && destination.Equals("Paris", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private sealed record BudgetNestsRoom(
        string room_id,
        string hotel_name,
        int room_type_code,
        decimal per_night_rate,
        int cancellation_tier,
        bool available);
}
