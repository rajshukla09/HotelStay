using System.Collections.Concurrent;
using HotelStay.Api.Models;

namespace HotelStay.Api.Stores;

public sealed class InMemoryReservationStore : IReservationStore
{
    private readonly ConcurrentDictionary<string, ReservationDetails> reservations = new(StringComparer.OrdinalIgnoreCase);

    public Task SaveAsync(ReservationDetails reservation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        reservations[reservation.Reference] = reservation;
        return Task.CompletedTask;
    }

    public Task<ReservationDetails?> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        reservations.TryGetValue(reference, out var reservation);
        return Task.FromResult(reservation);
    }
}
