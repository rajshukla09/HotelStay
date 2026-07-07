using HotelStay.Domain.Enums;

namespace HotelStay.Domain.Destinations;

public static class DestinationCatalog
{
    private static readonly DestinationDefinition[] destinations =
    [
        new("Sydney", DestinationCategory.Domestic),
        new("Melbourne", DestinationCategory.Domestic),
        new("London", DestinationCategory.International),
        new("Paris", DestinationCategory.International),
        new("Tokyo", DestinationCategory.International)
    ];

    public static IReadOnlyList<DestinationDefinition> Destinations => destinations;

    public static IReadOnlyList<string> DomesticDestinations => destinations
        .Where(destination => destination.Category == DestinationCategory.Domestic)
        .Select(destination => destination.Name)
        .ToArray();

    public static IReadOnlyList<string> InternationalDestinations => destinations
        .Where(destination => destination.Category == DestinationCategory.International)
        .Select(destination => destination.Name)
        .ToArray();

    public static bool TryGetCategory(string? destination, out DestinationCategory category)
    {
        var match = destinations.FirstOrDefault(knownDestination =>
            string.Equals(knownDestination.Name, destination?.Trim(), StringComparison.OrdinalIgnoreCase));

        category = match?.Category ?? default;
        return match is not null;
    }

    public static bool IsKnownDestination(string? destination) => TryGetCategory(destination, out _);
}

public sealed record DestinationDefinition(string Name, DestinationCategory Category);
