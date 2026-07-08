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
    private readonly IReservationDocumentStorage reservationDocumentStorage;

    public ReserveHotelCommandHandler(IDocumentValidationService documentValidationService, IReservationStore reservationStore, IReservationDocumentStorage reservationDocumentStorage)
    {
        this.documentValidationService = documentValidationService;
        this.reservationStore = reservationStore;
        this.reservationDocumentStorage = reservationDocumentStorage;
    }

    public async Task<OperationResult<ReservationResponse>> HandleAsync(ReserveHotelCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        if (string.IsNullOrWhiteSpace(request.RoomId))
        {
            return OperationResult<ReservationResponse>.Failure("RoomId is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Provider))
        {
            return OperationResult<ReservationResponse>.Failure("Provider is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            return OperationResult<ReservationResponse>.Failure("GuestName is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (request.CheckOut <= request.CheckIn)
        {
            return OperationResult<ReservationResponse>.Failure("Check-out date must be after check-in date.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (request.PerNightRate <= 0 || request.TotalPrice <= 0)
        {
            return OperationResult<ReservationResponse>.Failure("Reservation pricing must be greater than zero.", ApplicationStatusCodes.Status400BadRequest);
        }

        var validation = documentValidationService.Validate(request.Destination, request.DocumentType, request.DocumentNumber);
        if (!validation.Succeeded)
        {
            return OperationResult<ReservationResponse>.Failure(validation.ErrorMessage!, validation.StatusCode!.Value);
        }

        if (command.UploadedDocument is null)
        {
            return OperationResult<ReservationResponse>.Failure("Reservation document file is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        var reference = GenerateReference(request);
        var uploadedFileName = await reservationDocumentStorage.SaveAsync(reference, command.UploadedDocument, cancellationToken);
        var reservation = new ReservationDetails(
            reference,
            request.RoomId,
            request.Provider,
            request.Destination,
            validation.Value!,
            request.CheckIn,
            request.CheckOut,
            request.RoomType,
            request.PerNightRate,
            request.TotalPrice,
            request.CancellationPolicy,
            request.GuestName,
            request.DocumentType,
            MaskDocumentNumber(request.DocumentNumber),
            uploadedFileName,
            DateTimeOffset.UtcNow);

        await reservationStore.SaveAsync(reservation, cancellationToken);
        return OperationResult<ReservationResponse>.Success(new ReservationResponse(reference, "Reservation confirmed.", request.TotalPrice, request.CancellationPolicy));
    }

    private static string MaskDocumentNumber(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return string.Empty;
        }

        var visible = Math.Min(4, documentNumber.Length);
        return new string('•', Math.Max(0, documentNumber.Length - visible)) + documentNumber[^visible..];
    }

    private static string GenerateReference(ReservationRequest request)
    {
        var hash = Math.Abs(HashCode.Combine(request.RoomId, request.Provider, request.GuestName, request.CheckIn, request.CheckOut));
        return $"HST-{DateTimeOffset.UtcNow:yyyyMMdd}-{hash % 0xFFFFFF:X6}";
    }
}
