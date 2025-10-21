using TentaloWebShop.Models;
namespace TentaloWebShop.Services;

public class CartService
{
    private readonly HttpClient _http;
    private readonly RestDataService _rest;
    private readonly LocalStorageService _store;
    private readonly AuthService _auth;
    private const string KEY = "cart";
    private bool _initialized;

    public event Action? Changed;
    public List<CartItem> Items { get; private set; } = new();

    // Propiedades calculadas b�sicas
    public int TotalQuantity => Items.Sum(i => i.Quantity);

    // Base imponible (sin IVA, sin descuento en factura)
    public decimal BaseImponible => Items.Sum(i => i.Product.PriceFrom * i.Quantity);

    // Descuento en factura (se aplica sobre la base imponible)
    public decimal DescuentoFactura
    {
        get
        {
            var descuentoPorcentaje = _auth?.CurrentUser?.DescuentoFactura ?? 0;
            return BaseImponible * descuentoPorcentaje / 100;
        }
    }

    // Base imponible despu�s del descuento en factura
    public decimal BaseImponibleConDescuento => BaseImponible - DescuentoFactura;

    // Total sin IVA (con descuento en factura aplicado)
    public decimal TotalAmount => BaseImponibleConDescuento;

    // IVA calculado sobre la base imponible con descuento
    public decimal TotalVat => Items.Sum(i => CalculateItemVat(i));

    // Importe total (base con descuento + IVA)
    public decimal ImporteTotal => TotalAmount + TotalVat;

    public CartService(LocalStorageService store, RestDataService rest, AuthService auth)
    {
        _store = store;
        _rest = rest;
        _auth = auth;
    }
    // M�todo auxiliar para calcular IVA de un item espec�fico
    // Ahora aplica el descuento en factura proporcionalmente
    private decimal CalculateItemVat(CartItem item)
    {
        try
        {
            // Validar que el producto y sus propiedades no sean null
            if (item?.Product == null) return 0;

            // Calcular el subtotal del item (sin descuento)
            decimal itemSubtotal = item.Product.PriceFrom * item.Quantity;

            // Calcular la proporci�n del descuento en factura para esta l�nea
            decimal descuentoLineaFactura = 0;
            if (DescuentoFactura > 0 && BaseImponible > 0)
            {
                descuentoLineaFactura = (itemSubtotal / BaseImponible) * DescuentoFactura;
            }

            // Calcular la base imponible de la l�nea despu�s del descuento
            decimal baseLineaConDescuento = itemSubtotal - descuentoLineaFactura;

            // Validar y convertir el tipo de IVA
            decimal vatPercentage = GetVatPercentage(item.Product.TipoIva);

            // Calcular el IVA sobre la base con descuento
            return baseLineaConDescuento * (vatPercentage / 100);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculando IVA para producto {item?.Product?.Id}: {ex.Message}");
            return 0;
        }
    }

    // M�todo auxiliar para obtener el porcentaje de IVA de forma segura
    private decimal GetVatPercentage(object tipoIva)
    {
        if (tipoIva == null) return 0;

        // Intentar diferentes tipos de conversi�n
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

        // Si no se puede convertir, intentar Convert.ToDecimal como �ltimo recurso
        try
        {
            return Convert.ToDecimal(tipoIva);
        }
        catch
        {
            return 0;
        }
    }
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            Items = await _store.GetAsync<List<CartItem>>(KEY) ?? new();
        }
        catch
        {
            Items = new();
        }

        _initialized = true;
        Changed?.Invoke();
    }

    public async Task Add(Product product, int quantity = 1)
    {
        var existing = Items.FirstOrDefault(x => x.Product.Id == product.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem { Product = product, Quantity = quantity });
        }
        await SaveAndNotify();
    }

    public async Task Update(string productId, int newQuantity)
    {
        var item = Items.FirstOrDefault(x => x.Product.Id == productId);
        if (item != null)
        {
            if (newQuantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = newQuantity;
            }
            await SaveAndNotify();
        }
    }

    public async Task Remove(string productId)
    {
        var item = Items.FirstOrDefault(x => x.Product.Id == productId);
        if (item != null)
        {
            Items.Remove(item);
            await SaveAndNotify();
        }
    }

    public async Task Clear()
    {
        Items.Clear();
        await SaveAndNotify();
    }

    public async Task<Status> ProcessOrder(List<CartItem> carro, string direnvio, string usuario, string cliente)
    {
        var status = new Status();

        // Obtener el descuento en factura del usuario
        var descuentoCabecera = (double)(_auth?.CurrentUser?.DescuentoFactura ?? 0);

        // Pasar directamente el carrito (List<CartItem>) al RestDataService
        // RestDataService se encargar� de crear los BufferPedidos
        var eprods = await _rest.PedidoVenta(
            carro,              // List<CartItem> - el carrito tal cual
            cliente,            // string cliente
            descuentoCabecera,  // double descuentoCabecera
            "",                 // string Observaciones
            direnvio,           // string direnvio
            usuario             // string usuario
        );

        if (eprods != null)
        {
            status = eprods;
        }
        else
        {
            status.IsSuccess = false;
            status.Message = "Error al procesar el pedido.";
        }

        return status;
    }
    private async Task SaveAndNotify()
    {
        await _store.SetAsync(KEY, Items);
        Changed?.Invoke();
    }
    private void NotifyChanged() => Changed?.Invoke();
}

 