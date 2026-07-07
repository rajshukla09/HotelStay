using HotelStay.Api.Models;
using HotelStay.Api.Providers;
using HotelStay.Api.Services;

namespace HotelStay.Api.Queries;

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
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("Destination is required.", StatusCodes.Status400BadRequest);
        }

        if (query.CheckIn is null)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("CheckIn is required.", StatusCodes.Status400BadRequest);
        }

        if (query.CheckOut is null)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("CheckOut is required.", StatusCodes.Status400BadRequest);
        }

        if (query.CheckOut.Value <= query.CheckIn.Value)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("CheckOut must be after CheckIn.", StatusCodes.Status400BadRequest);
        }

        var providerResults = await Task.WhenAll(providers.Select(provider =>
            provider.SearchAsync(query.Destination, query.CheckIn.Value, query.CheckOut.Value, query.RoomType, cancellationToken)));

        var results = providerResults
            .SelectMany(result => result)
            .OrderBy(room => room.TotalPrice)
            .ToArray();

        return OperationResult<IReadOnlyList<HotelRoomResult>>.Success(results);
    }
}
