using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using __RootNamespace__.API;
using __RootNamespace__.Application;
using __RootNamespace__.Host.Extensions;
using __RootNamespace__.Host.Middleware;
using __RootNamespace__.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "__RootNamespace__.Api");

    var seqUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqUrl))
    {
        var apiKey = context.Configuration["Seq:ApiKey"];
        loggerConfiguration.WriteTo.Seq(
            seqUrl,
            apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey);
    }
});

// Configuramos en minúsculas las rutas de los controladores.
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers(options =>
    {
        // Prefijo "api" global — en el controlador solo se necesita [Route("[controller]")] (heredado de BaseApiController).
        options.Conventions.Add(new ApiPrefixConvention("api"));
    })
    .AddApplicationPart(typeof(BaseApiController).Assembly)
    .AddNewtonsoftJson(opt =>
    {
        opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        opt.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
        opt.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomCorsPolicy", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");

        var corsOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();
        if (corsOrigins != null)
        {
            policy.WithOrigins(corsOrigins);
        }
        else
        {
            policy.AllowAnyOrigin();
        }
    });
});

var applicationName = builder.Configuration["ApplicationName"];

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"{applicationName} API",
        Version = "v1",
        Description = $"API de {applicationName}"
    });

    c.CustomSchemaIds(x => x.FullName);
    c.EnableAnnotations();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, introduzca un token válido",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddCustomServices(builder.Environment, builder.Configuration);
builder.Services.AddAuthServices(builder.Configuration, builder.Environment);
builder.Services.AddJwtService(builder.Configuration);

builder.Services.AddDbServices(builder.Configuration, builder.Environment);

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(sqlConnection))
{
    healthChecks.AddSqlServer(sqlConnection, name: "sqlserver", tags: new[] { "db" });
}

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

app.UseCors("CustomCorsPolicy");

if (builder.Configuration.GetValue("SwaggerAvailable", false))
{
    app.UseSwagger()
    .UseSwaggerUI(options =>
    {
        if (!string.IsNullOrEmpty(pathBase))
        {
            options.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", $"{applicationName} API v1");
        }
        else
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{applicationName} API v1");
        }

        options.HeadContent = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"/>";
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseMiddleware<SecurityHeadersMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString(),
                    exception = e.Value.Exception?.Message,
                    tags = e.Value.Tags
                })
        });
    }
});

app.UseStaticFiles();

app.Run();
