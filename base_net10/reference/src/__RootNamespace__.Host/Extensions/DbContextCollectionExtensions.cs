using __RootNamespace__.Infrastructure.Context;

namespace __RootNamespace__.Host.Extensions;

public static class DbContextCollectionExtensions
{
    public static IServiceCollection AddDbServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var connectionString = config.GetConnectionString("DefaultConnection") ?? throw new Exception("No Database Connection String provided");
        var timeOutConnection = config.GetConnectionString("TimeOutConnection") ?? throw new Exception("No Database Connection String provided");

        services.AddSingleton<DapperContext>();

        return services;
    }
}
