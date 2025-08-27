using Microsoft.JSInterop;

namespace TentaloWebShop.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    public LocalStorageService(IJSRuntime js) => _js = js;

    public async Task<T?> GetAsync<T>(string key)
        => await _js.InvokeAsync<T?>("localStore.get", key);

    public async Task SetAsync<T>(string key, T value)
        => await _js.InvokeVoidAsync("localStore.set", key, value);

    public async Task RemoveAsync(string key)
        => await _js.InvokeVoidAsync("localStore.remove", key);
}