using HotelStay.Application.Models;
using HotelStay.Domain.Entities;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelStay.Api.Swagger;

public sealed class HotelStaySchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(ReservationRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["roomId"] = new OpenApiString("PremierStays-Sydney-ps-standard"),
                ["provider"] = new OpenApiString("PremierStays"),
                ["destination"] = new OpenApiString("Sydney"),
                ["checkIn"] = new OpenApiString("2026-07-08"),
                ["checkOut"] = new OpenApiString("2026-07-10"),
                ["roomType"] = new OpenApiString("Standard"),
                ["perNightRate"] = new OpenApiDouble(145),
                ["totalPrice"] = new OpenApiDouble(290),
                ["guestName"] = new OpenApiString("Alex Guest"),
                ["documentType"] = new OpenApiString("NationalId"),
                ["documentNumber"] = new OpenApiString("NAT-123456"),
                ["cancellationPolicy"] = new OpenApiString("FreeCancellation48Hours")
            };
        }
        else if (context.Type == typeof(ReservationResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["reference"] = new OpenApiString("HST-20260707-ABC123"),
                ["message"] = new OpenApiString("Reservation confirmed."),
                ["totalPrice"] = new OpenApiDouble(290),
                ["cancellationPolicy"] = new OpenApiString("FreeCancellation48Hours")
            };
        }
        else if (context.Type == typeof(HotelRoomResult))
        {
            schema.Example = new OpenApiObject
            {
                ["roomId"] = new OpenApiString("PremierStays-Sydney-ps-standard"),
                ["provider"] = new OpenApiString("PremierStays"),
                ["hotelName"] = new OpenApiString("Sydney Premier Hotel"),
                ["destination"] = new OpenApiString("Sydney"),
                ["roomType"] = new OpenApiString("Standard"),
                ["perNightRate"] = new OpenApiDouble(145),
                ["totalPrice"] = new OpenApiDouble(290),
                ["cancellationPolicy"] = new OpenApiString("FreeCancellation48Hours"),
                ["amenities"] = new OpenApiArray { new OpenApiString("WiFi"), new OpenApiString("Breakfast") },
                ["starRating"] = new OpenApiInteger(4)
            };
        }
        else if (context.Type == typeof(ReservationDetails))
        {
            schema.Example = new OpenApiObject
            {
                ["reference"] = new OpenApiString("HST-20260707-ABC123"),
                ["roomId"] = new OpenApiString("PremierStays-Sydney-ps-standard"),
                ["provider"] = new OpenApiString("PremierStays"),
                ["destination"] = new OpenApiString("Sydney"),
                ["destinationCategory"] = new OpenApiString("Domestic"),
                ["checkIn"] = new OpenApiString("2026-07-08"),
                ["checkOut"] = new OpenApiString("2026-07-10"),
                ["roomType"] = new OpenApiString("Standard"),
                ["perNightRate"] = new OpenApiDouble(145),
                ["totalPrice"] = new OpenApiDouble(290),
                ["cancellationPolicy"] = new OpenApiString("FreeCancellation48Hours"),
                ["guestName"] = new OpenApiString("Alex Guest"),
                ["documentType"] = new OpenApiString("NationalId"),
                ["documentNumber"] = new OpenApiString("NAT-123456"),
                ["createdAt"] = new OpenApiString("2026-07-07T00:00:00Z")
            };
        }
    }
}
