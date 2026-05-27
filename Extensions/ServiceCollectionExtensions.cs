using Asp.Versioning;
using backend_api_dotnet9.Data;
using backend_api_dotnet9.Infrastructure;
using backend_api_dotnet9.Services;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace backend_api_dotnet9.Extensions;

public static class ServiceCollectionExtensions
{
    public const string ClientCorsPolicy = "ClientCorsPolicy";

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddControllers();
        services.AddProblemDetails();
        services.AddHealthChecks();
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            options.UseNpgsql(ConnectionStringHelper.NormalizePostgresConnectionString(connectionString));
        });
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IPrintTypeService, PrintTypeService>();
        services.AddScoped<IProductConfigurationService, ProductConfigurationService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "backend-api-dotnet9",
                Version = "v1",
                Description = "API documentation for backend-api-dotnet9"
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy(ClientCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    return;
                }

                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        });

        services.AddOpenApi();
        return services;
    }
}
