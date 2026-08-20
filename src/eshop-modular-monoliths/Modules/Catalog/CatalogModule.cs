using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Data;
using Shared.Data.Interceptors;

namespace Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        
        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>()!);
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<Shared.Data.Seed.IDataSeeder, Catalog.data.Seed.CatalogDataSeeder>();
        
        return services;
    }
    
    public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
    {
        app.UseMigration<CatalogDbContext>();
        return app;
    }
}