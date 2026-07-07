using HotelStay.Application.Interfaces;
using HotelStay.Infrastructure.Providers;
using HotelStay.Infrastructure.Services;
using HotelStay.Infrastructure.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace HotelStay.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHotelStayInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IHotelProvider, PremierStaysProvider>();
        services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
        services.AddSingleton<IDocumentValidationService, DocumentValidationService>();
        services.AddSingleton<IReservationStore, InMemoryReservationStore>();

        return services;
    }
}
