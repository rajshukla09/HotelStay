using HotelStay.Api.Commands;
using HotelStay.Api.Models;
using HotelStay.Api.Queries;
using HotelStay.Api.Services;

namespace HotelStay.Api.Endpoints;

public static class HotelEndpoints
{
    public static IEndpointRouteBuilder MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/hotels/search", SearchHotelsAsync)
            .WithName("SearchHotels")
            .WithOpenApi();

        app.MapPost("/hotels/reserve", ReserveHotelAsync)
            .WithName("ReserveHotel")
            .WithOpenApi();

        app.MapGet("/hotels/reservation/{reference}", GetReservationAsync)
            .WithName("GetReservation")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> SearchHotelsAsync(
        string? destination,
        DateOnly? checkIn,
        DateOnly? checkOut,
        RoomType? roomType,
        SearchHotelsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return Results.BadRequest(new { error = "ValidationFailed", message = "Destination is required." });
        }

        if (checkIn is null)
        {
            return Results.BadRequest(new { error = "ValidationFailed", message = "CheckIn is required." });
        }

        if (checkOut is null)
        {
            return Results.BadRequest(new { error = "ValidationFailed", message = "CheckOut is required." });
        }

        var result = await handler.HandleAsync(new SearchHotelsQuery(destination, checkIn.Value, checkOut.Value, roomType), cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> ReserveHotelAsync(
        ReservationRequest request,
        ReserveHotelCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ReserveHotelCommand(request), cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> GetReservationAsync(
        string reference,
        GetReservationByReferenceQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetReservationByReferenceQuery(reference), cancellationToken);
        return ToHttpResult(result);
    }

    private static IResult ToHttpResult<T>(OperationResult<T> result) => result.Succeeded
        ? Results.Ok(result.Value)
        : Results.Json(new { error = "ValidationFailed", message = result.ErrorMessage }, statusCode: result.StatusCode);
}
