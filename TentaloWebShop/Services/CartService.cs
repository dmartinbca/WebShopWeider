using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class CartService
{
    private readonly LocalStorageService _store;
    private const string KEY = "cart";
    private bool _initialized;

    public event Action? Changed;
    public List<CartItem> Items { get; private set; } = new();
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal TotalAmount => Items.Sum(i => i.Product.PriceFrom * i.Quantity);

    public CartService(LocalStorageService store) => _store = store;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try { Items = await _store.GetAsync<List<CartItem>>(KEY) ?? new(); }
        catch { Items = new(); }
        _initialized = true;
        Changed?.Invoke();
    }

    private async Task Save()
    {
        await _store.SetAsync(KEY, Items);
        Changed?.Invoke();
    }

    public async Task Add(Product product, int quantity)
    {
        await InitializeAsync();
        var line = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (line is null) Items.Add(new CartItem { Product = product, Quantity = quantity });
        else line.Quantity += quantity;
        await Save();
    }

    public async Task Update(string productId, int quantity)
    {
        await InitializeAsync();
        var line = Items.FirstOrDefault(i => i.Product.Id == productId);
        if (line is null) return;
        line.Quantity = Math.Max(1, quantity);
        await Save();
    }

    public async Task Remove(string productId)
    {
        await InitializeAsync();
        Items.RemoveAll(i => i.Product.Id == productId);
        await Save();
    }

    public async Task Clear()
    {
        await InitializeAsync();
        Items.Clear();
        await Save();
    }
}
