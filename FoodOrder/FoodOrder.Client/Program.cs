using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FoodOrder.Client.Services;
using FoodOrder.Client.Storage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IKeyValueStorage, LocalStorageKeyValueStorage>();
builder.Services.AddScoped<ISessionService, LocalStorageSessionService>();
builder.Services.AddScoped<IMenuService, LocalStorageMenuService>();
builder.Services.AddScoped<ICartService, LocalStorageCartService>();
builder.Services.AddScoped<IOrderService, LocalStorageOrderService>();

await builder.Build().RunAsync();