using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Data.Seed;

namespace Shared.Data;

public static class Extensions
{
    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        MigrationDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();
        SeedDataAsync(app.ApplicationServices).GetAwaiter().GetResult();
        return app;
    }
    
    public static async Task MigrationDatabaseAsync<TContext>(IServiceProvider serviceProvider) where TContext : DbContext 
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync();
    }

    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
        foreach (var seeder in seeders)
        {
            await seeder.SeedAllAsync();
        }
    }
}