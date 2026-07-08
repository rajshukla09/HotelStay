using HotelStay.Domain.Entities;
using HotelStay.Application.Commands;
using HotelStay.Application.Models;
using HotelStay.Domain.Enums;
using HotelStay.Application.Queries;
using HotelStay.Application.Common;
using HotelStay.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
    [ProducesResponseType(ApplicationStatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(ApplicationStatusCodes.Status400BadRequest)]
    [ProducesResponseType(ApplicationStatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReservationResponse>> ReserveAsync(
        [FromForm] ReservationFormRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Error", message = "Reservation form data is required." });
        }

        var fileValidation = ValidateDocumentFile(request.DocumentFile);
        if (fileValidation is not null)
        {
            return BadRequest(new { error = "Error", message = fileValidation });
        }

        var reservationRequest = request.ToReservationRequest();
        var uploadedDocument = new UploadedReservationDocument(
            request.DocumentFile!.FileName,
            request.DocumentFile.ContentType,
            request.DocumentFile.Length,
            () => request.DocumentFile.OpenReadStream());

        var result = await reserveHotelCommandHandler.HandleAsync(new ReserveHotelCommand(reservationRequest, uploadedDocument), cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("reservation/{reference}")]
    [ProducesResponseType(typeof(ReservationDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(ApplicationStatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDetails>> GetReservationAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var result = await getReservationByReferenceQueryHandler.HandleAsync(
            new GetReservationByReferenceQuery(reference),
            cancellationToken);

        return ToActionResult(result);
    }


    private static string? ValidateDocumentFile(IFormFile? file)
    {
        const long MaxFileSize = 5 * 1024 * 1024;
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/pdf", "image/jpeg", "image/png" };

        if (file is null || file.Length == 0)
        {
            return "Reservation document file is required.";
        }

        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(file.ContentType))
        {
            return "Reservation document must be a PDF, JPG, or PNG file.";
        }

        if (file.Length > MaxFileSize)
        {
            return "Reservation document must be 5 MB or smaller.";
        }

        return null;
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


public sealed class ReservationFormRequest
{
    [Required]
    public string RoomId { get; set; } = string.Empty;

    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public RoomType RoomType { get; set; }

    public decimal PerNightRate { get; set; }

    public decimal TotalPrice { get; set; }

    [Required]
    public string GuestName { get; set; } = string.Empty;

    public DocumentType DocumentType { get; set; }

    [Required]
    public string DocumentNumber { get; set; } = string.Empty;

    public CancellationPolicy CancellationPolicy { get; set; } = CancellationPolicy.Flexible24Hours;

    public IFormFile? DocumentFile { get; set; }

    public ReservationRequest ToReservationRequest() => new(
        RoomId,
        Provider,
        Destination,
        CheckIn,
        CheckOut,
        RoomType,
        PerNightRate,
        TotalPrice,
        GuestName,
        DocumentType,
        DocumentNumber,
        CancellationPolicy);
}
