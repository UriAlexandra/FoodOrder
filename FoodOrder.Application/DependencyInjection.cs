using FoodOrder.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrder.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetMenuHandler>();
        services.AddScoped<AddDishHandler>();
        services.AddScoped<SetDishAvailabilityHandler>();
        services.AddScoped<ListCouriersHandler>();
        services.AddScoped<AddCourierHandler>();
        services.AddScoped<SetCourierActiveHandler>();
        services.AddScoped<PlaceOrderHandler>();
        services.AddScoped<ChangeOrderStatusHandler>();
        services.AddScoped<AssignCourierHandler>();
        services.AddScoped<ListOrdersHandler>();
        services.AddScoped<TrackOrderHandler>();

        return services;
    }
}