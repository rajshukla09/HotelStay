using HotelStay.Api.Models;

namespace HotelStay.Api.Providers;

public sealed class BudgetNestsProvider : IHotelProvider
{
    public string ProviderName => "BudgetNests";

    public Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(string destination, DateOnly checkIn, DateOnly checkOut, RoomType? roomType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var responses = new[]
        {
            new BudgetNestsRoom("bn-standard", ProviderName, $"{destination} Budget Nest", RoomType.Standard, 95m, CancellationPolicy.Flexible24Hours, true),
            new BudgetNestsRoom("bn-deluxe", ProviderName, $"{destination} City Nest", RoomType.Deluxe, 155m, CancellationPolicy.NonRefundable, IsAvailable(destination, RoomType.Deluxe)),
            new BudgetNestsRoom("bn-suite", ProviderName, $"{destination} Family Nest", RoomType.Suite, 240m, CancellationPolicy.Flexible24Hours, false)
        };

        var results = responses
            .Where(room => room.available)
            .Where(room => roomType is null || room.room_type == roomType)
            .Select(room => new HotelRoomResult(
                RoomId: $"{ProviderName}-{destination}-{room.room_id}".Replace(' ', '-'),
                Provider: room.provider_name,
                HotelName: room.hotel_name,
                Destination: destination,
                RoomType: room.room_type,
                PerNightRate: room.per_night_rate,
                TotalPrice: room.per_night_rate * nights,
                CancellationPolicy: room.cancellation_policy,
                Amenities: null,
                StarRating: null))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<HotelRoomResult>>(results);
    }

    private static bool IsAvailable(string destination, RoomType roomType) =>
        Math.Abs(HashCode.Combine(destination.ToUpperInvariant(), roomType)) % 2 == 0;

    private sealed record BudgetNestsRoom(
        string room_id,
        string provider_name,
        string hotel_name,
        RoomType room_type,
        decimal per_night_rate,
        CancellationPolicy cancellation_policy,
        bool available);
}
