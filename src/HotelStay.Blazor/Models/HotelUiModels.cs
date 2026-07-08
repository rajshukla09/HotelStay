using Microsoft.AspNetCore.Components.Forms;
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
    string DocumentNumber,
    IBrowserFile DocumentFile)
{
    public MultipartFormDataContent ToMultipartFormData()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(Room.RoomId), nameof(ReservationRequest.RoomId) },
            { new StringContent(Room.Provider), nameof(ReservationRequest.Provider) },
            { new StringContent(Room.Destination), nameof(ReservationRequest.Destination) },
            { new StringContent(CheckIn.ToString("yyyy-MM-dd")), nameof(ReservationRequest.CheckIn) },
            { new StringContent(CheckOut.ToString("yyyy-MM-dd")), nameof(ReservationRequest.CheckOut) },
            { new StringContent(Room.RoomType.ToString()), nameof(ReservationRequest.RoomType) },
            { new StringContent(Room.PerNightRate.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(ReservationRequest.PerNightRate) },
            { new StringContent(Room.TotalPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(ReservationRequest.TotalPrice) },
            { new StringContent(GuestName), nameof(ReservationRequest.GuestName) },
            { new StringContent(DocumentType.ToString()), nameof(ReservationRequest.DocumentType) },
            { new StringContent(DocumentNumber), nameof(ReservationRequest.DocumentNumber) },
            { new StringContent(Room.CancellationPolicy.ToString()), nameof(ReservationRequest.CancellationPolicy) }
        };

        content.Add(new StreamContent(DocumentFile.OpenReadStream(5 * 1024 * 1024)), "DocumentFile", DocumentFile.Name);
        return content;
    }

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
        DocumentNumber,
        Room.CancellationPolicy);
}
