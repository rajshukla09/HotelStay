using HotelStay.Application.Interfaces;
using HotelStay.Application.Models;
using Microsoft.AspNetCore.Hosting;

namespace HotelStay.Infrastructure.Services;

public sealed class LocalReservationDocumentStorage : IReservationDocumentStorage
{
    private readonly string storagePath;

    public LocalReservationDocumentStorage(IWebHostEnvironment environment)
    {
        storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "ReservationDocuments");
    }

    public async Task<string> SaveAsync(string reservationReference, UploadedReservationDocument document, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(storagePath);
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
        var safeFileName = $"{reservationReference}{extension}";
        var path = Path.Combine(storagePath, safeFileName);

        await using var source = document.OpenReadStream();
        await using var destination = File.Create(path);
        await source.CopyToAsync(destination, cancellationToken);
        return safeFileName;
    }
}
