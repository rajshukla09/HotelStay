using HotelStay.Blazor.Models;

namespace HotelStay.Blazor.Services;

public interface IApiClient
{
    Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default);

    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken = default);
}
