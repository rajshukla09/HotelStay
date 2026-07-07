using HotelStay.Domain.Entities;

namespace HotelStay.Application.Interfaces;

public interface IReservationStore
{
    Task SaveAsync(ReservationDetails reservation, CancellationToken cancellationToken);
    Task<ReservationDetails?> GetByReferenceAsync(string reference, CancellationToken cancellationToken);
}
