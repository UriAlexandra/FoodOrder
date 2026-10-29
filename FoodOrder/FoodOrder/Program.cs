using FoodOrder.Api;
using FoodOrder.Application;
using FoodOrder.Application.Ports;
using FoodOrder.Components;
using FoodOrder.Endpoints;
using FoodOrder.Hubs;
using FoodOrder.Infrastructure;
using FoodOrder.Infrastructure.Identity;
using FoodOrder.Infrastructure.Persistence;
using FoodOrder.Shared.Realtime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Rétegek
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);   // adatbázis, Identity, e-mail-outbox
builder.Services.AddSingleton<IClock, SystemClock>();

// Bejelentkezés (süti), szerepkör-policy-k, aktuális felhasználó
builder.Services.AddFoodOrderAuth();

// Valós idejű rendelésértesítés (SignalR)
builder.Services.AddSignalR();
builder.Services.AddSingleton<IOrderUpdatePublisher, SignalROrderUpdatePublisher>();

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

app.UseSecurityHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();

    // Fejlesztői adatbázis: függő migrációk, mintaadatok és tesztfiókok (csak Developmentben!).
    // A „Testing” környezetben (integrációs tesztek) ez nem fut: a tesztek maguk hozzák létre az adatbázist.
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

// A HTML-oldalaknál a 404 a magyar „nem található” oldalra visz. Az API- és SignalR-válaszokat nem írjuk át:
// ott a státuszkód (és a ProblemDetails) a válasz.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.Use((context, next) =>
{
    if (AuthSetup.IsApiRequest(context.Request.Path) && context.Features.Get<IStatusCodePagesFeature>() is { } statusCodePages)
        statusCodePages.Enabled = false;
    return next(context);
});

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

// SignalR hub
app.MapHub<OrderHub>(OrderHubContract.Path);

app.Run();

// Az integrációs tesztek (WebApplicationFactory) ezen a típuson keresztül indítják az alkalmazást.
public partial class Program { }
