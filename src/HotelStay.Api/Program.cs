using HotelStay.Api.Middleware;
using HotelStay.Application.Commands;
using HotelStay.Application.Queries;
using HotelStay.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHotelStayInfrastructure();
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

app.MapControllers();

app.Run();

public partial class Program;
