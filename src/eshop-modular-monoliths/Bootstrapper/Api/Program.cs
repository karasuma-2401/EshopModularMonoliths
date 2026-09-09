using Carter;
using Scalar.AspNetCore;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var catalogAssembly = typeof(CatalogModule).Assembly;
var basketAssembly = typeof(BasketModule).Assembly;
var orderingAssembly = typeof(OrderingModule).Assembly;

builder.Services.AddCarterWithAssemblies(catalogAssembly, basketAssembly, orderingAssembly);
builder.Services.AddMediatRWithAssemblies(catalogAssembly, basketAssembly, orderingAssembly);


builder.Services
    .AddCatalogModule(builder.Configuration)
    .AddBasketModule(builder.Configuration)
    .AddOrderingModule(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapCarter();

app
    .UseCatalogModule()
    .UseBasketModule()
    .UseOrderingModule();


app.Run();