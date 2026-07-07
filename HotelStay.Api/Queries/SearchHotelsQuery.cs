using HotelStay.Api.Models;

namespace HotelStay.Api.Queries;

public sealed record SearchHotelsQuery(string? Destination, DateOnly? CheckIn, DateOnly? CheckOut, RoomType? RoomType);
