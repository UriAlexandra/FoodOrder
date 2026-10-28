using FoodOrder.Api;
using FoodOrder.Application;
using FoodOrder.Application.Ports;
using FoodOrder.Components;
using FoodOrder.Endpoints;
using FoodOrder.Infrastructure;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Rétegek
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);   // adatbázis + ASP.NET Core Identity
builder.Services.AddSingleton<IClock, SystemClock>();

// Bejelentkezés (süti), szerepkör-policy-k, aktuális felhasználó
builder.Services.AddFoodOrderAuth();

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

    // Fejlesztői adatbázis: függő migrációk, mintaadatok és tesztfiókok (csak Developmentben!)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}
else
{
    app.UseHsts();
}

// A HTML-oldalaknál a 404 a „nem található” oldalra visz; az API-válaszokat (/api) nem írjuk át.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FoodOrder.Client._Imports).Assembly);

// API végpontok
app.MapAuthEndpoints();
app.MapDishesEndpoints();
app.MapCouriersEndpoints();
app.MapOrdersEndpoints();

app.Run();
