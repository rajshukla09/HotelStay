using HotelStay.Application.Models;

namespace HotelStay.Application.Interfaces;

public interface IReservationDocumentStorage
{
    Task<string> SaveAsync(string reservationReference, UploadedReservationDocument document, CancellationToken cancellationToken);
}
