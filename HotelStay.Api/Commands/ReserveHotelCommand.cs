using HotelStay.Api.Models;

namespace HotelStay.Api.Commands;

public sealed record ReserveHotelCommand(ReservationRequest Request);
