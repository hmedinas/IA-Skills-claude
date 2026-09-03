using Microsoft.Extensions.DependencyInjection;
using __RootNamespace__.Infrastructure.Persistence;

namespace __RootNamespace__.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();

        return services;
    }
}
