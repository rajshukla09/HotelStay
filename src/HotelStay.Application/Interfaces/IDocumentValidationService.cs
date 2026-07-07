using HotelStay.Application.Common;
using HotelStay.Domain.Enums;

namespace HotelStay.Application.Interfaces;

public interface IDocumentValidationService
{
    OperationResult<DestinationCategory> Validate(string destination, DocumentType documentType, string documentNumber);
}
