using HotelStay.Api.Models;

namespace HotelStay.Api.Stores;

public interface IReservationStore
{
    Task SaveAsync(ReservationDetails reservation, CancellationToken cancellationToken);
    Task<ReservationDetails?> GetByReferenceAsync(string reference, CancellationToken cancellationToken);
}
