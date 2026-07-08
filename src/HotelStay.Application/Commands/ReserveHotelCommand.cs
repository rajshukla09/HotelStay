using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Application.Commands;

public sealed record ReserveHotelCommand(ReservationRequest Request, UploadedReservationDocument? UploadedDocument = null);
