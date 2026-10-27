using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FoodOrder.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<FoodOrderStore>();

await builder.Build().RunAsync();