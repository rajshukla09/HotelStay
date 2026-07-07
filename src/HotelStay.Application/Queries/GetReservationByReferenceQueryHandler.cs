using HotelStay.Domain.Entities;
using HotelStay.Application.Models;
using HotelStay.Application.Common;
using HotelStay.Domain.Enums;
using HotelStay.Application.Interfaces;

namespace HotelStay.Application.Queries;

public sealed class GetReservationByReferenceQueryHandler
{
    private readonly IReservationStore reservationStore;

    public GetReservationByReferenceQueryHandler(IReservationStore reservationStore)
    {
        this.reservationStore = reservationStore;
    }

    public async Task<OperationResult<ReservationDetails>> HandleAsync(GetReservationByReferenceQuery query, CancellationToken cancellationToken)
    {
        var reservation = await reservationStore.GetByReferenceAsync(query.Reference, cancellationToken);
        return reservation is null
            ? OperationResult<ReservationDetails>.Failure("Reservation not found.", ApplicationStatusCodes.Status404NotFound)
            : OperationResult<ReservationDetails>.Success(reservation);
    }
}
