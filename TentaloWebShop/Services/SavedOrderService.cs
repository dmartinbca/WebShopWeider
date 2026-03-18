using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class SavedOrderService
{
    private readonly LocalStorageService _store;
    private readonly CartService _cart;
    private readonly AuthService _auth;
    private const string KEY = "saved_orders";

    public SavedOrderService(LocalStorageService store, CartService cart, AuthService auth)
    {
        _store = store;
        _cart = cart;
        _auth = auth;
    }

    public async Task<List<SavedOrder>> GetAllAsync()
    {
        var all = await _store.GetAsync<List<SavedOrder>>(KEY) ?? new();
        var customerNo = GetEffectiveCustomerNo();
        if (string.IsNullOrWhiteSpace(customerNo))
            return all;
        return all.Where(o => o.CustomerNo == customerNo).ToList();
    }

    public async Task<SavedOrder> SaveCurrentCartAsync(string name)
    {
        var all = await _store.GetAsync<List<SavedOrder>>(KEY) ?? new();

        var saved = new SavedOrder
        {
            Name = name,
            CustomerNo = GetEffectiveCustomerNo(),
            Items = _cart.Items.Select(CloneCartItem).ToList(),
            Observaciones = _cart.Observaciones
        };

        all.Add(saved);
        await _store.SetAsync(KEY, all);
        return saved;
    }

    public async Task LoadToCartAsync(string savedOrderId)
    {
        var all = await _store.GetAsync<List<SavedOrder>>(KEY) ?? new();
        var order = all.FirstOrDefault(o => o.Id == savedOrderId);
        if (order == null) return;

        // Limpiar carrito actual
        await _cart.Clear();

        // Cargar items del pedido guardado
        foreach (var item in order.Items)
        {
            await _cart.Add(item.Product, item.Quantity, item.DescuentoProducto);
        }

        // Restaurar observaciones
        _cart.Observaciones = order.Observaciones;
    }

    public async Task DeleteAsync(string savedOrderId)
    {
        var all = await _store.GetAsync<List<SavedOrder>>(KEY) ?? new();
        all.RemoveAll(o => o.Id == savedOrderId);
        await _store.SetAsync(KEY, all);
    }

    public async Task RenameAsync(string savedOrderId, string newName)
    {
        var all = await _store.GetAsync<List<SavedOrder>>(KEY) ?? new();
        var order = all.FirstOrDefault(o => o.Id == savedOrderId);
        if (order != null)
        {
            order.Name = newName;
            await _store.SetAsync(KEY, all);
        }
    }

    private string GetEffectiveCustomerNo()
    {
        return _auth?.CurrentCustomer?.CustNo ?? _auth?.CurrentUser?.CustomerNo ?? "";
    }

    private static CartItem CloneCartItem(CartItem source)
    {
        return new CartItem
        {
            Product = source.Product,
            Quantity = source.Quantity,
            DescuentoProducto = source.DescuentoProducto,
            PackId = source.PackId,
            PromoCode = source.PromoCode,
            PackLineType = source.PackLineType,
            PackDescription = source.PackDescription,
            IsAthleteCredit = source.IsAthleteCredit,
            OriginalDescuento = source.OriginalDescuento
        };
    }
}
