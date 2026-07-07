using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IDocumentValidationService
{
    OperationResult<DestinationCategory> Validate(string destination, DocumentType documentType, string documentNumber);
}
