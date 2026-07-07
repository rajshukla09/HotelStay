using HotelStay.Api.Models;

namespace HotelStay.Api.Providers;

public interface IHotelProvider
{
    string ProviderName { get; }

    Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(
        string destination,
        DateOnly checkIn,
        DateOnly checkOut,
        RoomType? roomType,
        CancellationToken cancellationToken);
}
