namespace FoodOrder.Client.Storage;

public interface IKeyValueStorage
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T item);
    Task RemoveItemAsync(string key);
}