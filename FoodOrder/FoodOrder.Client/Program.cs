using FoodOrder.Client.Services;
using FoodOrder.Client.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// API-hívások ugyanarra a szerverre, amelyik a klienst kiszolgálja (a hitelesítő süti automatikusan megy)
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Bejelentkezési állapot a szerver sütije alapján
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<IMenuService, ApiMenuService>();
builder.Services.AddScoped<IOrderService, ApiOrderService>();
builder.Services.AddScoped<ICourierService, ApiCourierService>();

// Valós idejű rendelésértesítések (SignalR) és felugró értesítések
builder.Services.AddScoped<OrderUpdatesClient>();
builder.Services.AddScoped<ToastService>();

// Böngészőben maradó állapot: a felhasználó kosara
builder.Services.AddScoped<IKeyValueStorage, LocalStorageKeyValueStorage>();
builder.Services.AddScoped<ICartService, LocalStorageCartService>();

await builder.Build().RunAsync();
