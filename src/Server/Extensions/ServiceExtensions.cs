using Services.SalesOrders;

namespace Server.Extensions;

/// <summary>
/// Registers application service dependencies.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds feature services used by the application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="webRootPath">Server web root path used by file-backed services.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string webRootPath)
    {
        services.AddScoped<ISalesOrderService>(_ => new SalesOrderService(webRootPath));
        return services;
    }
}