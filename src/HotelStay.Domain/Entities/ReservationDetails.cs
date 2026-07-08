using HotelStay.Domain.Enums;

namespace HotelStay.Domain.Entities;

public sealed record ReservationDetails(
    string Reference,
    string RoomId,
    string Provider,
    string Destination,
    DestinationCategory DestinationCategory,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    string UploadedDocumentFileName,
    DateTimeOffset CreatedAt);
