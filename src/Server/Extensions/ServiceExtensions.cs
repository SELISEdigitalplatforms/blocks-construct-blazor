using Services.SalesOrders;

namespace Server.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string webRootPath)
    {
        services.AddScoped<ISalesOrderService>(_ => new SalesOrderService(webRootPath));
        return services;
    }
}
