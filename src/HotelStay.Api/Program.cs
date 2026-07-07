using System.Text.Json.Serialization;
using HotelStay.Api.Middleware;
using HotelStay.Api.Swagger;
using HotelStay.Application.Commands;
using HotelStay.Application.Queries;
using HotelStay.Infrastructure.DependencyInjection;

const string BlazorClientCorsPolicy = "BlazorClient";

var builder = WebApplication.CreateBuilder(args);
var blazorClientOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(BlazorClientCorsPolicy, policy =>
    {
        policy
            .WithOrigins(blazorClientOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<HotelStaySchemaExampleFilter>();
});
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
app.UseCors(BlazorClientCorsPolicy);

app.MapControllers();

app.Run();

public partial class Program;
