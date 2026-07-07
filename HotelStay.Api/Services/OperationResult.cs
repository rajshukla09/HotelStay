namespace HotelStay.Api.Services;

public sealed record OperationResult<T>(T? Value, string? ErrorMessage, int? StatusCode)
{
    public bool Succeeded => ErrorMessage is null;
    public static OperationResult<T> Success(T value) => new(value, null, null);
    public static OperationResult<T> Failure(string message, int statusCode) => new(default, message, statusCode);
}
