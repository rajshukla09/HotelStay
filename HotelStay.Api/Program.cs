using HotelStay.Api.Commands;
using HotelStay.Api.Endpoints;
using HotelStay.Api.Middleware;
using HotelStay.Api.Providers;
using HotelStay.Api.Queries;
using HotelStay.Api.Services;
using HotelStay.Api.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
builder.Services.AddSingleton<IDocumentValidationService, DocumentValidationService>();
builder.Services.AddSingleton<IReservationStore, InMemoryReservationStore>();
builder.Services.AddScoped<SearchHotelsQueryHandler>();
builder.Services.AddScoped<GetReservationByReferenceQueryHandler>();
builder.Services.AddScoped<ReserveHotelCommandHandler>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.MapHotelEndpoints();

app.Run();

public partial class Program;
