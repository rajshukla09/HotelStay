namespace HotelStay.Application.Models;

public sealed record UploadedReservationDocument(
    string FileName,
    string ContentType,
    long Length,
    Func<Stream> OpenReadStream);

public sealed record ReservationDocumentMetadata(
    string DocumentType,
    string MaskedDocumentNumber,
    string UploadedFileName);
