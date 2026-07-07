namespace HotelStay.Api.Models;

public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}

public enum DocumentType
{
    Passport,
    NationalId
}

public enum CancellationPolicy
{
    FreeCancellation48Hours,
    Flexible24Hours,
    NonRefundable
}

public enum DestinationCategory
{
    Domestic,
    International
}

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
    string Message);

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
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber,
    DateTimeOffset CreatedAt);
