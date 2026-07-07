namespace HotelStay.Api.Services;

public sealed record OperationResult<T>(T? Value, string? ErrorMessage, int? StatusCode, string? ErrorCode)
{
    public bool Succeeded => ErrorMessage is null;
    public static OperationResult<T> Success(T value) => new(value, null, null, null);
    public static OperationResult<T> Failure(string message, int statusCode, string errorCode = "ValidationFailed") => new(default, message, statusCode, errorCode);
}
