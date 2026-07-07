using HotelStay.Application.Models;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;

namespace HotelStay.Blazor.Services;

public sealed class HotelApiService : IHotelApiService
{
    private readonly IApiClient apiClient;

    public HotelApiService(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<ApiResult<IReadOnlyList<HotelRoomResult>>> SearchHotelsAsync(
        string? destination,
        DateOnly? checkIn,
        DateOnly? checkOut,
        RoomType? roomType,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryParameter(query, "destination", destination);
        AddQueryParameter(query, "checkIn", checkIn?.ToString("yyyy-MM-dd"));
        AddQueryParameter(query, "checkOut", checkOut?.ToString("yyyy-MM-dd"));
        AddQueryParameter(query, "roomType", roomType?.ToString());

        var requestUri = query.Count == 0
            ? "api/hotels/search"
            : $"api/hotels/search?{string.Join('&', query)}";

        return apiClient.GetAsync<IReadOnlyList<HotelRoomResult>>(requestUri, cancellationToken);
    }

    public Task<ApiResult<ReservationResponse>> ReserveHotelAsync(ReservationRequest request, CancellationToken cancellationToken = default)
        => apiClient.PostAsync<ReservationRequest, ReservationResponse>("api/hotels/reserve", request, cancellationToken);

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
