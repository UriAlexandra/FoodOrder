using FoodOrder.Api;
using FoodOrder.Application;
using FoodOrder.Application.Ports;
using FoodOrder.Components;
using FoodOrder.Endpoints;
using FoodOrder.Infrastructure;
using FoodOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Rétegek
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IClock, SystemClock>();

// Hibakezelés: minden kivételből ProblemDetails válasz lesz
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

// Blazor (WebAssembly kliens)
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();

    // Fejlesztői adatbázis: függő migrációk lefuttatása és mintaadatok betöltése
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}
else
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FoodOrder.Client._Imports).Assembly);

// API végpontok
app.MapDishesEndpoints();
app.MapCouriersEndpoints();
app.MapOrdersEndpoints();

app.Run();
