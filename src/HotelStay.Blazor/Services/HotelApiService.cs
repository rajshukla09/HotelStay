using HotelStay.Application.Models;
using HotelStay.Blazor.Models;
using HotelStay.Domain.Entities;

namespace HotelStay.Blazor.Services;

public sealed class HotelApiService : IHotelApiService
{
    private readonly IApiClient apiClient;

    public HotelApiService(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<ApiResult<IReadOnlyList<HotelRoomResult>>> SearchHotelsAsync(SearchHotelsRequest request, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryParameter(query, "destination", request.Destination);
        AddQueryParameter(query, "checkIn", request.CheckIn.ToString("yyyy-MM-dd"));
        AddQueryParameter(query, "checkOut", request.CheckOut.ToString("yyyy-MM-dd"));
        AddQueryParameter(query, "roomType", request.RoomType?.ToString());

        return apiClient.GetAsync<IReadOnlyList<HotelRoomResult>>($"api/hotels/search?{string.Join('&', query)}", cancellationToken);
    }

    public Task<ApiResult<ReservationResponse>> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<ReservationRequest, ReservationResponse>("api/hotels/reserve", request.ToApiRequest(), cancellationToken);

    public Task<ApiResult<ReservationDetails>> GetReservationAsync(string reference, CancellationToken cancellationToken = default)
        => apiClient.GetAsync<ReservationDetails>($"api/hotels/reservation/{Uri.EscapeDataString(reference)}", cancellationToken);

    private static void AddQueryParameter(List<string> query, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        query.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
    }
}
