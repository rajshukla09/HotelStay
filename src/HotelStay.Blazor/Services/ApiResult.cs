namespace HotelStay.Blazor.Services;

public sealed record ApiResult<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorMessage,
    int StatusCode)
{
    public static ApiResult<T> Success(T? data, int statusCode) => new(true, data, null, statusCode);

    public static ApiResult<T> Failure(string errorMessage, int statusCode) => new(false, default, errorMessage, statusCode);
}
