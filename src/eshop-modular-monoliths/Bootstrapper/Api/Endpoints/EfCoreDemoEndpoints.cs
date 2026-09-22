using System.Diagnostics;
using Carter;
using Catalog.data;
using Catalog.Products.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ordering.Data;
using Ordering.Orders.Models;
using Ordering.Orders.ValueObjects;

namespace Api.Endpoints;

public class EfCoreDemoEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/demo/efcore")
            .WithTags("EF Core Seminar Live Demo");

        // 1. Seed 500 - 1000 Products & So sánh Batching vs Vòng lặp SaveChanges
        group.MapPost("/seed-products", SeedProductsBenchmark)
            .WithName("DemoSeedProducts")
            .WithSummary("1. Demo Batching: So sánh AddRange vs Vòng lặp SaveChanges (Seed 500-1000 items)");

        // 2. So sánh Tracking vs AsNoTracking (RAM snapshot & CPU)
        group.MapGet("/tracking-vs-asnotracking", TrackingVsAsNoTrackingBenchmark)
            .WithName("DemoTrackingVsAsNoTracking")
            .WithSummary("2. Demo Hiệu năng: Change Tracker (AsTracking) vs AsNoTracking trên 1000 records");

        // 3. Demo N+1 Query vs Eager Loading (Include)
        group.MapGet("/n-plus-one", NPlusOneBenchmark)
            .WithName("DemoNPlusOneQuery")
            .WithSummary("3. Demo Lỗi kinh điển: N+1 Query vs Eager Loading (.Include)");

        // 4. Demo Deferred Execution (Khi nào SQL thực sự gửi đi)
        group.MapGet("/deferred-execution", DeferredExecutionDemo)
            .WithName("DemoDeferredExecution")
            .WithSummary("4. Demo LINQ to Entities: Deferred Execution & ToQueryString()");
    }

    private static async Task<IResult> SeedProductsBenchmark(
        [FromQuery] int count,
        [FromQuery] bool compareLoop,
        [FromServices] IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        if (count <= 0) count = 1000;
        if (count > 2000) count = 2000;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var existingCount = await db.Products.CountAsync(cancellationToken);
        
        long? loopElapsedMs = null;
        int loopCount = 50; // Thử 50 items bằng vòng lặp SaveChanges để thấy độ trễ
        
        if (compareLoop)
        {
            var loopStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < loopCount; i++)
            {
                var p = Product.Create(
                    Guid.NewGuid(),
                    $"Loop-Product-{existingCount + i}",
                    new List<string> { "Demo", "Slow" },
                    "Tạo bằng vòng lặp SaveChanges() từng item",
                    "slow.png",
                    100 + i);

                db.Products.Add(p);
                await db.SaveChangesAsync(cancellationToken); // 1 round-trip mạng tới PostgreSQL cho mỗi item!
            }
            loopStopwatch.Stop();
            loopElapsedMs = loopStopwatch.ElapsedMilliseconds;
        }

        // Batch Insert phần còn lại bằng AddRange + 1 lần SaveChangesAsync()
        int remainingToSeed = count;
        var batchStopwatch = Stopwatch.StartNew();
        var batchProducts = new List<Product>();
        
        for (int i = 0; i < remainingToSeed; i++)
        {
            var p = Product.Create(
                Guid.NewGuid(),
                $"Batch-Product-{existingCount + (loopElapsedMs.HasValue ? loopCount : 0) + i}",
                new List<string> { "Demo", "Fast", "Electronics" },
                $"Sản phẩm demo hiệu năng số #{i + 1} - Tạo bằng AddRange()",
                "fast.png",
                Math.Round((decimal)(50 + (i % 100) * 1.5), 2));

            batchProducts.Add(p);
        }

        db.Products.AddRange(batchProducts);
        await db.SaveChangesAsync(cancellationToken); // EF Core tự gom thành batch SQL duy nhất!
        batchStopwatch.Stop();

        var totalInDb = await db.Products.CountAsync(cancellationToken);

        return Results.Ok(new
        {
            Title = "Demo 1: Bulk Insert & Cơ chế Batching của EF Core",
            TotalProductsInDb = totalInDb,
            BatchInsert = new
            {
                ItemsInserted = remainingToSeed,
                ExecutionTimeMs = batchStopwatch.ElapsedMilliseconds,
                Method = "db.Products.AddRange() + 1 lần SaveChangesAsync()",
                Efficiency = "EF Core tự động gom các câu INSERT vào các Batch command, giảm tối đa round-trip mạng."
            },
            LoopInsert = loopElapsedMs.HasValue ? new
            {
                ItemsInserted = loopCount,
                ExecutionTimeMs = loopElapsedMs.Value,
                Method = "Vòng lặp for: db.Products.Add() + SaveChangesAsync() từng item",
                Warning = $"Chỉ {loopCount} items mà mất {loopElapsedMs.Value} ms vì phải mở transaction và gửi request {loopCount} lần!"
            } : null,
            KeyTakeaway = "Luôn dùng AddRange() hoặc Bulk Extensions thay vì gọi SaveChanges() trong vòng lặp!"
        });
    }

    private static async Task<IResult> TrackingVsAsNoTrackingBenchmark(
        [FromQuery] int count,
        [FromServices] IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        if (count <= 0) count = 1000;

        // 1. Đo lường WITH TRACKING (Mặc định)
        using var scope1 = scopeFactory.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<CatalogDbContext>();
        
        var sw1 = Stopwatch.StartNew();
        var trackedProducts = await db1.Products.AsTracking().Take(count).ToListAsync(cancellationToken);
        sw1.Stop();
        var trackedCount = db1.ChangeTracker.Entries().Count();

        // 2. Đo lường WITH AsNoTracking()
        using var scope2 = scopeFactory.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<CatalogDbContext>();
        
        var sw2 = Stopwatch.StartNew();
        var untrackedProducts = await db2.Products.AsNoTracking().Take(count).ToListAsync(cancellationToken);
        sw2.Stop();
        var untrackedCount = db2.ChangeTracker.Entries().Count();

        var speedup = sw2.ElapsedMilliseconds > 0 
            ? Math.Round((double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds, 2) 
            : 1.0;

        return Results.Ok(new
        {
            Title = "Demo 2: Hiệu năng Change Tracker - AsTracking vs AsNoTracking",
            RecordsFetched = trackedProducts.Count,
            WithTracking = new
            {
                ExecutionTimeMs = sw1.ElapsedMilliseconds,
                TrackedEntitiesInRam = trackedCount,
                Explanation = "EF Core phải tạo 1 bản sao snapshot cho từng entity trên RAM để sẵn sàng theo dõi thay đổi khi gọi SaveChanges(). Tốn RAM và CPU."
            },
            WithAsNoTracking = new
            {
                ExecutionTimeMs = sw2.ElapsedMilliseconds,
                TrackedEntitiesInRam = untrackedCount,
                Explanation = "Tắt hoàn toàn Change Tracker. EF Core chỉ map dữ liệu trực tiếp sang C# Object. Không snapshot, không tốn RAM."
            },
            Comparison = new
            {
                SpeedupRatio = $"{speedup}x",
                MemorySaved = $"{trackedCount} snapshot objects không bị giữ trên RAM"
            },
            GoldenRule = "Với các tác vụ chỉ ĐỌC (GET API, xem danh sách, báo cáo, export), LUÔN LUÔN dùng AsNoTracking()!"
        });
    }

    private static async Task<IResult> NPlusOneBenchmark(
        [FromQuery] int cartCount,
        [FromServices] IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        if (cartCount <= 0) cartCount = 10;
        if (cartCount > 50) cartCount = 50;

        // Đảm bảo có ít nhất cartCount giỏ hàng có items để demo
        await EnsureSeedShoppingCarts(scopeFactory, cartCount, cancellationToken);

        // 1. Minh họa N+1 Queries (Anti-pattern kinh điển: Lấy Carts rồi dùng vòng lặp query Items)
        using var scope1 = scopeFactory.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<Basket.Data.BasketDbContext>();

        var swNPlusOne = Stopwatch.StartNew();
        // Câu query số 1: Lấy danh sách ShoppingCarts
        var carts = await db1.ShoppingCarts
            .OrderBy(c => c.UserName)
            .Take(cartCount)
            .ToListAsync(cancellationToken);
        
        // N câu query tiếp theo: Từng cart lại bắn 1 câu SQL riêng để lấy Items
        int itemQueriesCount = 0;
        var loadedItems = new List<object>();
        foreach (var cart in carts)
        {
            var items = await db1.ShoppingCartItems
                .Where(i => i.ShoppingCartId == cart.Id)
                .ToListAsync(cancellationToken);
            itemQueriesCount++;
        }
        swNPlusOne.Stop();
        int totalSqlNPlusOne = 1 + itemQueriesCount;

        // 2. Minh họa Eager Loading với .Include() (Cách chuẩn EF Core)
        using var scope2 = scopeFactory.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<Basket.Data.BasketDbContext>();

        var swEager = Stopwatch.StartNew();
        // Đúng 1 câu SQL duy nhất dùng LEFT JOIN
        var eagerCarts = await db2.ShoppingCarts
            .Include(c => c.Items)
            .OrderBy(c => c.UserName)
            .Take(cartCount)
            .ToListAsync(cancellationToken);
        swEager.Stop();
        int totalSqlEager = 1;

        return Results.Ok(new
        {
            Title = "Demo 3: Bài toán kinh điển N+1 Query và Giải pháp Eager Loading (.Include)",
            CartsRequested = cartCount,
            AntiPattern_NPlusOne = new
            {
                TotalSqlSentToDb = totalSqlNPlusOne,
                Formula = $"1 (SELECT ShoppingCarts) + {itemQueriesCount} (SELECT ShoppingCartItems cho từng Cart) = {totalSqlNPlusOne} câu SQL",
                ExecutionTimeMs = swNPlusOne.ElapsedMilliseconds,
                Warning = "Xem màn hình Terminal: EF Core bắn liên tiếp 11 câu SELECT riêng biệt tới PostgreSQL! Khi dữ liệu lớn, database sẽ quá tải kết nối."
            },
            Solution_EagerLoading = new
            {
                TotalSqlSentToDb = totalSqlEager,
                Syntax = "db.ShoppingCarts.Include(c => c.Items).Take(N).ToListAsync()",
                ExecutionTimeMs = swEager.ElapsedMilliseconds,
                Advantage = "Xem màn hình Terminal: EF Core tự sinh câu lệnh SQL LEFT JOIN và gửi ĐÚNG 1 CÂU SQL duy nhất!"
            },
            KeyTakeaway = "Tránh truy vấn con trong vòng lặp foreach! Luôn dùng .Include() (Eager Loading) hoặc .Select() Projection để gom dữ liệu trong 1 query."
        });
    }

    private static async Task<IResult> DeferredExecutionDemo(
        [FromServices] IServiceScopeFactory scopeFactory,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        // 1. Tạo câu truy vấn IQueryable từng bước (chưa bắn SQL)
        IQueryable<Product> query = db.Products.AsNoTracking();

        // Bước 1: Filter
        query = query.Where(p => p.Price > 50);

        // Bước 2: Sort
        query = query.OrderByDescending(p => p.Price);

        // Bước 3: Paging
        query = query.Take(5);

        // Lấy câu lệnh SQL thực tế mà EF Core biên dịch sẵn (chưa gửi tới DB!)
        string generatedSql = query.ToQueryString();

        // Bước 4: Thực thi truy vấn (Materialization) - SQL chỉ THỰC SỰ được gửi đi tại thời điểm này!
        var sw = Stopwatch.StartNew();
        var results = await query.ToListAsync(cancellationToken);
        sw.Stop();

        return Results.Ok(new
        {
            Title = "Demo 4: Deferred Execution (Thực thi trì hoãn) trong EF Core",
            Concept = "IQueryable<T> chỉ là định nghĩa câu truy vấn (Expression Tree), CHƯA HỀ gửi lệnh nào tới Database cho tới khi bạn gọi ToListAsync(), FirstOrDefaultAsync(), CountAsync(),...",
            GeneratedSql = generatedSql,
            MaterializedCount = results.Count,
            ExecutionTimeMs = sw.ElapsedMilliseconds,
            Explanation = "Nhờ Deferred Execution, chúng ta có thể thoải mái nối thêm Where, OrderBy, Skip, Take linh hoạt theo điều kiện lọc của người dùng mà không lo truy vấn thừa vào Database!"
        });
    }

    private static async Task EnsureSeedShoppingCarts(
        IServiceScopeFactory scopeFactory,
        int count,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Basket.Data.BasketDbContext>();

        var existing = await db.ShoppingCarts.CountAsync(cancellationToken);
        if (existing >= count) return;

        for (int i = 0; i < count - existing; i++)
        {
            var cart = Basket.Models.ShoppingCart.Create(Guid.NewGuid(), $"demo_user_{i + 1:D3}");
            cart.AddItem(Guid.NewGuid(), 2, "Black", 1200, $"iPhone 16 Pro #{i + 1}");
            cart.AddItem(Guid.NewGuid(), 1, "White", 800, $"Sony WH-1000XM5 #{i + 1}");
            cart.AddItem(Guid.NewGuid(), 3, "Blue", 45, $"USB-C Cable #{i + 1}");

            db.ShoppingCarts.Add(cart);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
