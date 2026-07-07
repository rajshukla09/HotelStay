using HotelStay.Api.Commands;
using HotelStay.Api.Models;
using HotelStay.Api.Queries;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelStay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HotelsController : ControllerBase
{
    private readonly SearchHotelsQueryHandler searchHotelsQueryHandler;
    private readonly ReserveHotelCommandHandler reserveHotelCommandHandler;
    private readonly GetReservationByReferenceQueryHandler getReservationByReferenceQueryHandler;

    public HotelsController(
        SearchHotelsQueryHandler searchHotelsQueryHandler,
        ReserveHotelCommandHandler reserveHotelCommandHandler,
        GetReservationByReferenceQueryHandler getReservationByReferenceQueryHandler)
    {
        this.searchHotelsQueryHandler = searchHotelsQueryHandler;
        this.reserveHotelCommandHandler = reserveHotelCommandHandler;
        this.getReservationByReferenceQueryHandler = getReservationByReferenceQueryHandler;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<HotelRoomResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<HotelRoomResult>>> SearchAsync(
        [FromQuery] string? destination,
        [FromQuery] DateOnly? checkIn,
        [FromQuery] DateOnly? checkOut,
        [FromQuery] RoomType? roomType,
        CancellationToken cancellationToken)
    {
        var result = await searchHotelsQueryHandler.HandleAsync(
            new SearchHotelsQuery(destination, checkIn, checkOut, roomType),
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost("reserve")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReservationResponse>> ReserveAsync(
        [FromBody] ReservationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await reserveHotelCommandHandler.HandleAsync(new ReserveHotelCommand(request), cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("reservation/{reference}")]
    [ProducesResponseType(typeof(ReservationDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDetails>> GetReservationAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var result = await getReservationByReferenceQueryHandler.HandleAsync(
            new GetReservationByReferenceQuery(reference),
            cancellationToken);

        return ToActionResult(result);
    }

    private ActionResult<T> ToActionResult<T>(OperationResult<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return StatusCode(
            result.StatusCode ?? StatusCodes.Status500InternalServerError,
            new { error = result.ErrorCode, message = result.ErrorMessage });
    }
}
