using System.Text.Json;
using Microsoft.JSInterop;

namespace FoodOrder.Client.Storage;

public class LocalStorageKeyValueStorage : IKeyValueStorage
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageKeyValueStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json)) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T item)
    {
        var json = JsonSerializer.Serialize(item);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}