using TentaloWebShop.Models;
namespace TentaloWebShop.Services;

public class CartService
{
    private readonly HttpClient _http;
   
    private readonly RestDataService _rest;
   
    private readonly LocalStorageService _store;
    private const string KEY = "cart";
    private bool _initialized;
    public event Action? Changed;
    public List<CartItem> Items { get; private set; } = new();
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal TotalAmount => Items.Sum(i => i.Product.PriceFrom * i.Quantity);

    // Cálculo de IVA mejorado con validaciones
    public decimal TotalVat => Items.Sum(i => CalculateItemVat(i));

    public decimal ImporteTotal => TotalAmount + TotalVat;

    public CartService(LocalStorageService store, RestDataService rest)
    {
        _store = store;
        _rest = rest;
    }
    // Método auxiliar para calcular IVA de un item específico
    private decimal CalculateItemVat(CartItem item)
    {
        try
        {
            // Validar que el producto y sus propiedades no sean null
            if (item?.Product == null) return 0;

            // Calcular el subtotal del item
            decimal itemSubtotal = item.Product.PriceFrom * item.Quantity;

            // Validar y convertir el tipo de IVA
            decimal vatPercentage = GetVatPercentage(item.Product.TipoIva);

            // Calcular el IVA
            return itemSubtotal * (vatPercentage / 100);
        }
        catch (Exception ex)
        {
            // Log del error si tienes logging configurado
            Console.WriteLine($"Error calculando IVA para producto {item?.Product?.Id}: {ex.Message}");
            return 0;
        }
    }

    // Método auxiliar para obtener el porcentaje de IVA de forma segura
    private decimal GetVatPercentage(object tipoIva)
    {
        if (tipoIva == null) return 0;

        // Intentar diferentes tipos de conversión
        switch (tipoIva)
        {
            case decimal d:
                return d;
            case int i:
                return i;
            case double db:
                return (decimal)db;
            case float f:
                return (decimal)f;
            case string s:
                if (decimal.TryParse(s, out decimal result))
                    return result;
                break;
        }

        // Si no se puede convertir, intentar Convert.ToDecimal como último recurso
        try
        {
            return Convert.ToDecimal(tipoIva);
        }
        catch
        {
            return 0; // Default a 0% si no se puede convertir
        }
    }

    // Método para obtener detalles de IVA por tipo (útil para debugging)
    public Dictionary<decimal, decimal> GetVatBreakdown()
    {
        return Items
            .GroupBy(i => GetVatPercentage(i.Product.TipoIva))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(i => CalculateItemVat(i))
            );
    }

    // Método para debugging - obtener detalles del cálculo
    public string GetCalculationDetails()
    {
        var details = new System.Text.StringBuilder();
        details.AppendLine("=== DETALLES DEL CARRITO ===");

        foreach (var item in Items)
        {
            decimal subtotal = item.Product.PriceFrom * item.Quantity;
            decimal vatRate = GetVatPercentage(item.Product.TipoIva);
            decimal vatAmount = CalculateItemVat(item);

            details.AppendLine($"Producto: {item.Product.Name}");
            details.AppendLine($"  Precio: {item.Product.PriceFrom:C} x {item.Quantity} = {subtotal:C}");
            details.AppendLine($"  IVA: {vatRate}% = {vatAmount:C}");
            details.AppendLine();
        }

        details.AppendLine($"SUBTOTAL: {TotalAmount:C}");
        details.AppendLine($"IVA TOTAL: {TotalVat:C}");
        details.AppendLine($"IMPORTE TOTAL: {ImporteTotal:C}");

        return details.ToString();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
           // GetCalculationDetails();
            return; 
        }
             
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
    public async Task<Status> ProcessOrder(List<CartItem> carro,string direnvio,string usuario,string cliente)
    {
        var status = new Status();
        var eprods = await _rest.PedidoVenta(carro, "", direnvio, usuario, cliente);
        if (eprods != null )
        {
            status= eprods;
        }
        else
        {
            status.IsSuccess = false;
            status.Message = "Error al procesar el pedido.";
        }
        return status;

         
    }


}