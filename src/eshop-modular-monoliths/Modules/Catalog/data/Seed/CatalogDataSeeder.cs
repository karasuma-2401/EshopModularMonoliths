using Shared.Data.Seed;

namespace Catalog.data.Seed;

public class CatalogDataSeeder(CatalogDbContext dbContext): IDataSeeder
{
    public async Task SeedAllAsync()
    {
        if (!await dbContext.Products.AnyAsync())
        {
            await dbContext.AddRangeAsync(InitialData.Products);
            await dbContext.SaveChangesAsync();
        }
    }
}