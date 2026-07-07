using HotelStay.Application.Models;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;

namespace HotelStay.Blazor.Services;

public interface IHotelApiService
{
    Task<ApiResult<IReadOnlyList<HotelRoomResult>>> SearchHotelsAsync(
        string? destination,
        DateOnly? checkIn,
        DateOnly? checkOut,
        RoomType? roomType,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ReservationResponse>> ReserveHotelAsync(ReservationRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<ReservationDetails>> GetReservationAsync(string reference, CancellationToken cancellationToken = default);
}
