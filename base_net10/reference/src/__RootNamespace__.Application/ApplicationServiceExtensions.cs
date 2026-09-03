using Microsoft.Extensions.DependencyInjection;

namespace __RootNamespace__.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(c =>
            c.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly));

        return services;
    }
}
