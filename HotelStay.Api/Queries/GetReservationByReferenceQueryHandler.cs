using HotelStay.Api.Models;
using HotelStay.Api.Services;
using HotelStay.Api.Stores;

namespace HotelStay.Api.Queries;

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
            ? OperationResult<ReservationDetails>.Failure("Reservation not found.", StatusCodes.Status404NotFound)
            : OperationResult<ReservationDetails>.Success(reservation);
    }
}
