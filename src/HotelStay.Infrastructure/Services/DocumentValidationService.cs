using HotelStay.Application.Common;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Domain.Destinations;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Services;

public sealed class DocumentValidationService : IDocumentValidationService
{
    public OperationResult<DestinationCategory> Validate(string destination, DocumentType documentType, string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return OperationResult<DestinationCategory>.Failure("DocumentNumber is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (!DestinationCatalog.TryGetCategory(destination, out var category))
        {
            return OperationResult<DestinationCategory>.Failure("Destination is not supported.", ApplicationStatusCodes.Status422UnprocessableEntity);
        }

        if (category == DestinationCategory.International && documentType != DocumentType.Passport)
        {
            return OperationResult<DestinationCategory>.Failure("International destinations require a Passport document.", ApplicationStatusCodes.Status422UnprocessableEntity);
        }

        return OperationResult<DestinationCategory>.Success(category);
    }
}
