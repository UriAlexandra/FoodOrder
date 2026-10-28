using FoodOrder.Client.Services;
using FoodOrder.Client.Storage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// API-hívások ugyanarra a szerverre, amelyik a klienst kiszolgálja
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ApiClient>();

builder.Services.AddScoped<IMenuService, ApiMenuService>();
builder.Services.AddScoped<IOrderService, ApiOrderService>();
builder.Services.AddScoped<ICourierService, ApiCourierService>();

// Böngészőben maradó állapot: kosár és a demó-szerepkör
builder.Services.AddScoped<IKeyValueStorage, LocalStorageKeyValueStorage>();
builder.Services.AddScoped<ICartService, LocalStorageCartService>();
builder.Services.AddScoped<ISessionService, LocalStorageSessionService>();

await builder.Build().RunAsync();
