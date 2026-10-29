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

// A HTML-oldalaknál a 404 a „nem található” oldalra visz; az API- és SignalR-válaszokat nem írjuk át.
app.UseWhen(
    context => !AuthSetup.IsApiRequest(context.Request.Path),
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

// SignalR hub
app.MapHub<OrderHub>(OrderHubContract.Path);

app.Run();
