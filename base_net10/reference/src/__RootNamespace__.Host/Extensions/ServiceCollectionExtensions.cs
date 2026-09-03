using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using __RootNamespace__.Common.Auth;
using System.Text;

namespace __RootNamespace__.Host.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
    {
        services.AddCustomAuthentication(config, environment);
        services.AddCustomAuthorization(config);

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
    {
        // Punto de extensión para validaciones adicionales de autenticación por producto.
        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, IConfiguration config)
    {
        // Punto de extensión para políticas de autorización (roles/claims) por producto.
        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        #region Configuración para archivos grandes (subidas)

        services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = configuration.GetValue<int>("AppSettings:MaxFileLenght", 5242880);
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        #endregion

        return services;
    }

    public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
    {
        #region JWT

        services.AddSingleton(new JwtHandler(configuration));

        var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]!);

        services.AddAuthentication(configureOptions =>
        {
            configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        #endregion

        return services;
    }
}
