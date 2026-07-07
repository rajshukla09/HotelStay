using HotelStay.Application.Common;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Services;

public sealed class DocumentValidationService : IDocumentValidationService
{
    private static readonly HashSet<string> DomesticDestinations = new(StringComparer.OrdinalIgnoreCase) { "Sydney", "Melbourne" };
    private static readonly HashSet<string> InternationalDestinations = new(StringComparer.OrdinalIgnoreCase) { "London", "Paris", "Tokyo" };

    public OperationResult<DestinationCategory> Validate(string destination, DocumentType documentType, string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return OperationResult<DestinationCategory>.Failure("DocumentNumber is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        var category = DomesticDestinations.Contains(destination)
            ? DestinationCategory.Domestic
            : DestinationCategory.International;

        if (!DomesticDestinations.Contains(destination) && !InternationalDestinations.Contains(destination))
        {
            category = DestinationCategory.International;
        }

        if (category == DestinationCategory.International && documentType != DocumentType.Passport)
        {
            return OperationResult<DestinationCategory>.Failure("International destinations require a Passport document.", ApplicationStatusCodes.Status422UnprocessableEntity);
        }

        return OperationResult<DestinationCategory>.Success(category);
    }
}
