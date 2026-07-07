using HotelStay.Application.Models;
using HotelStay.Domain.Enums;

namespace HotelStay.Application.Queries;

public sealed record SearchHotelsQuery(string? Destination, DateOnly? CheckIn, DateOnly? CheckOut, RoomType? RoomType);
