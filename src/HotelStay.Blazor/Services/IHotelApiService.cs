using HotelStay.Application.Models;
using HotelStay.Blazor.Models;
using HotelStay.Domain.Entities;

namespace HotelStay.Blazor.Services;

public interface IHotelApiService
{
    Task<ApiResult<IReadOnlyList<HotelRoomResult>>> SearchHotelsAsync(SearchHotelsRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<ReservationResponse>> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<ReservationDetails>> GetReservationAsync(string reference, CancellationToken cancellationToken = default);
}
