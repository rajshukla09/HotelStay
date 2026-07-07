using HotelStay.Application.Models;

namespace HotelStay.Application.Interfaces;

public interface IHotelProvider
{
    string ProviderName { get; }

    Task<IReadOnlyCollection<HotelRoomResult>> SearchAsync(
        HotelSearchRequest request,
        CancellationToken cancellationToken);
}
