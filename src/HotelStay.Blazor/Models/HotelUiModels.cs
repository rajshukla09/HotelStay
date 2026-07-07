using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Blazor.Models;

public sealed record SearchHotelsRequest(
    string Destination,
    DateOnly CheckIn,
    DateOnly CheckOut,
    RoomType? RoomType);

public sealed record ReserveHotelRequest(
    HotelRoomResult Room,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string GuestName,
    DocumentType DocumentType,
    string DocumentNumber)
{
    public ReservationRequest ToApiRequest() => new(
        Room.RoomId,
        Room.Provider,
        Room.Destination,
        CheckIn,
        CheckOut,
        Room.RoomType,
        Room.PerNightRate,
        Room.TotalPrice,
        GuestName,
        DocumentType,
        DocumentNumber);
}
