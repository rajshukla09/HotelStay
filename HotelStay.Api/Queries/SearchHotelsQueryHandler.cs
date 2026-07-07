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
        if (query.CheckOut <= query.CheckIn)
        {
            return OperationResult<IReadOnlyList<HotelRoomResult>>.Failure("CheckOut must be after CheckIn.", StatusCodes.Status400BadRequest);
        }

        var providerResults = await Task.WhenAll(providers.Select(provider =>
            provider.SearchAsync(query.Destination, query.CheckIn, query.CheckOut, query.RoomType, cancellationToken)));

        var results = providerResults
            .SelectMany(result => result)
            .OrderBy(room => room.TotalPrice)
            .ToArray();

        return OperationResult<IReadOnlyList<HotelRoomResult>>.Success(results);
    }
}
