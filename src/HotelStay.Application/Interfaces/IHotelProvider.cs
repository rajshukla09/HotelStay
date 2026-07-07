using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Application.Interfaces;

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
