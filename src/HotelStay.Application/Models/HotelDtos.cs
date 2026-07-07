using HotelStay.Domain.Enums;

namespace HotelStay.Application.Models;

public sealed record HotelSearchRequest(
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType? RoomType);

public sealed record HotelRoomResult(
    string RoomId,
    string Provider,
    string HotelName,
    string Destination,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy,
    IReadOnlyList<string>? Amenities,
    int? StarRating);

public sealed record ReservationRequest(
    string RoomId,
    string Provider,
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType RoomType,
    decimal PerNightRate,
    decimal TotalPrice,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber);

public sealed record ReservationResponse(
    string Reference,
    string Message,
    decimal TotalPrice,
    CancellationPolicy CancellationPolicy);
