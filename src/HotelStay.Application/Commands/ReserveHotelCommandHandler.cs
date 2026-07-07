using HotelStay.Domain.Entities;
using HotelStay.Application.Models;
using HotelStay.Application.Common;
using HotelStay.Domain.Enums;
using HotelStay.Application.Interfaces;

namespace HotelStay.Application.Commands;

public sealed class ReserveHotelCommandHandler
{
    private readonly IDocumentValidationService documentValidationService;
    private readonly IReservationStore reservationStore;

    public ReserveHotelCommandHandler(IDocumentValidationService documentValidationService, IReservationStore reservationStore)
    {
        this.documentValidationService = documentValidationService;
        this.reservationStore = reservationStore;
    }

    public async Task<OperationResult<ReservationResponse>> HandleAsync(ReserveHotelCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            return OperationResult<ReservationResponse>.Failure("GuestName is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        var validation = documentValidationService.Validate(request.Destination, request.DocumentType, request.DocumentNumber);
        if (!validation.Succeeded)
        {
            return OperationResult<ReservationResponse>.Failure(validation.ErrorMessage!, validation.StatusCode!.Value);
        }

        var reference = GenerateReference(request);
        var reservation = new ReservationDetails(
            reference,
            request.RoomId,
            request.Provider,
            request.Destination,
            validation.Value.GetValueOrDefault(),
            request.CheckIn,
            request.CheckOut,
            request.RoomType,
            request.PerNightRate,
            request.TotalPrice,
            request.GuestName,
            request.DocumentType,
            request.DocumentNumber,
            DateTimeOffset.UtcNow);

        await reservationStore.SaveAsync(reservation, cancellationToken);
        return OperationResult<ReservationResponse>.Success(new ReservationResponse(reference, "Reservation confirmed."));
    }

    private static string GenerateReference(ReservationRequest request)
    {
        var hash = Math.Abs(HashCode.Combine(request.RoomId, request.Provider, request.GuestName, request.CheckIn, request.CheckOut));
        return $"HST-{DateTimeOffset.UtcNow:yyyyMMdd}-{hash % 0xFFFFFF:X6}";
    }
}
