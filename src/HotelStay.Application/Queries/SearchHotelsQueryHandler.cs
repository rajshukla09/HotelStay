using HotelStay.Application.Models;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Common;

namespace HotelStay.Application.Queries;

public sealed class SearchHotelsQueryHandler
{
    private readonly IEnumerable<IHotelProvider> providers;

    public SearchHotelsQueryHandler(IEnumerable<IHotelProvider> providers)
    {
        this.providers = providers;
    }

    public async Task<OperationResult<IReadOnlyList<HotelRoomResult>>> HandleAsync(SearchHotelsQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Destination))
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("Destination is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (query.CheckIn is null)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("Check-in date is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (query.CheckOut is null)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("Check-out date is required.", ApplicationStatusCodes.Status400BadRequest);
        }

        if (query.CheckOut.Value <= query.CheckIn.Value)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("Check-out date must be after check-in date.", ApplicationStatusCodes.Status400BadRequest);
        }

        var request = new HotelSearchRequest(
            query.Destination.Trim(),
            query.CheckIn.Value,
            query.CheckOut.Value,
            query.RoomType);

        var providerResults = await Task.WhenAll(providers.Select(provider =>
            provider.SearchAsync(request, cancellationToken)));

        var results = providerResults
            .SelectMany(result => result)
            .OrderBy(room => room.TotalPrice)
            .ToArray();

        return OperationResult<IReadOnlyList<HotelRoomResult>>.Success(results);
    }
}
