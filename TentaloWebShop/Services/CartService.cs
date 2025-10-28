using TentaloWebShop.Models;
namespace TentaloWebShop.Services;

public class CartService
{
    private readonly HttpClient _http;
    private readonly RestDataService _rest;
    private readonly LocalStorageService _store;
    private readonly AuthService _auth;
    private const string KEY = "cart";
    private const string KEY_OBSERVACIONES = "cart.observaciones";
    private bool _initialized;

    public event Action? Changed;
    public List<CartItem> Items { get; private set; } = new();

    // ✅ NUEVA PROPIEDAD: Observaciones del carrito
    private string _observaciones = "";
    public string Observaciones
    {
        get => _observaciones;
        set
        {
            _observaciones = value ?? "";
            SaveObservaciones();
            Changed?.Invoke();
        }
    }

    // Propiedades calculadas básicas
    public int TotalQuantity => Items.Sum(i => i.Quantity);

    // ✅ ACTUALIZADO: Base imponible usa SubtotalWithDiscount (respeta descuentos por línea)
    public decimal BaseImponible => Items.Sum(i => i.SubtotalWithDiscount);

    // Descuento en factura (se aplica sobre la base imponible)
    public decimal DescuentoFactura
    {
        get
        {
            var descuentoPorcentaje = _auth?.CurrentUser?.DescuentoFactura ?? 0;
            return BaseImponible * descuentoPorcentaje / 100;
        }
    }

    // ✅ NUEVO: Calcula el total de descuentos por producto (todas las líneas)
    public decimal DescuentoProductos
    {
        get
        {
            return Items.Sum(i =>
            {
                decimal precioOriginal = i.Product.PriceFrom * i.Quantity;
                decimal precioConDescuento = i.SubtotalWithDiscount;
                return precioOriginal - precioConDescuento;
            });
        }
    }

    // Base imponible después del descuento en factura
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

        // ✅ SUSCRIBIRSE AL EVENTO DE CAMBIO DE CLIENTE
        _auth.OnCustomerChanged += OnCustomerChanged;
    }

    // ✅ CAMBIO: de void a async Task
    private async Task OnCustomerChanged()
    {
        Console.WriteLine("[CartService.OnCustomerChanged] Limpiando carrito");
        await Clear();
    }

    // Método auxiliar para calcular IVA de un item específico
    // Ahora aplica el descuento en factura proporcionalmente
    private decimal CalculateItemVat(CartItem item)
    {
        try
        {
            // Validar que el producto y sus propiedades no sean null
            if (item?.Product == null) return 0;

            // Calcular el subtotal del item (sin descuento)
            decimal itemSubtotal = item.SubtotalWithDiscount;

            // Calcular la proporción del descuento en factura para esta línea
            decimal descuentoLineaFactura = 0;
            if (DescuentoFactura > 0 && BaseImponible > 0)
            {
                descuentoLineaFactura = (itemSubtotal / BaseImponible) * DescuentoFactura;
            }

            // Calcular la base imponible de la línea después del descuento
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
            _observaciones = await _store.GetAsync<string>(KEY_OBSERVACIONES) ?? "";
        }
        catch
        {
            Items = new();
            _observaciones = "";
        }

        _initialized = true;
        Changed?.Invoke();
    }

    // ========================================================================
    // ✅ MÉTODO Add() - ACTUALIZADO para soportar descuentos
    // Si no especificas descuento (0), se agrupa con otras líneas sin descuento
    // Si especificas descuento > 0, crea una LÍNEA SEPARADA
    // ========================================================================
    public async Task Add(Product product, int quantity = 1, decimal descuentoProducto = 0)
    {
        // Buscar SOLO por ProductId + DescuentoProducto
        // De esta forma, mismo producto con descuentos diferentes = líneas separadas
        var existing = Items.FirstOrDefault(x =>
            x.Product.Id == product.Id &&
            x.DescuentoProducto == descuentoProducto);

        if (existing != null)
        {
            // Mismo producto, MISMO descuento → agrupa
            existing.Quantity += quantity;
        }
        else
        {
            // Mismo producto, DIFERENTE descuento → nueva línea
            Items.Add(new CartItem
            {
                Product = product,
                Quantity = quantity,
                DescuentoProducto = descuentoProducto
            });
        }

        await SaveAndNotify();
    }

    // ========================================================================
    // ✅ MÉTODO AddWithDiscount() - Ahora SÍ crea líneas separadas
    // (Alias de Add() con parámetro descuento explícito)
    // ========================================================================
    public async Task AddWithDiscount(Product product, int quantity = 1, decimal descuentoProducto = 0)
    {
        // Simplemente llamar a Add() con los parámetros
        await Add(product, quantity, descuentoProducto);
    }

    // ========================================================================
    // ✅ MÉTODO Update() - ACTUALIZADO para identificar por ProductId + DescuentoProducto
    // Ahora necesitas especificar también el descuento para actualizar la cantidad
    // ========================================================================
    public async Task Update(string productId, int newQuantity, decimal descuentoProducto = 0)
    {
        // Buscar por AMBOS: ProductId AND DescuentoProducto
        var item = Items.FirstOrDefault(x =>
            x.Product.Id == productId &&
            x.DescuentoProducto == descuentoProducto);

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

    // ========================================================================
    // ✅ MÉTODO Remove() - ACTUALIZADO para identificar por ProductId + DescuentoProducto
    // Ahora necesitas especificar también el descuento para eliminar la línea correcta
    // ========================================================================
    public async Task Remove(string productId, decimal descuentoProducto = 0)
    {
        // Buscar por AMBOS: ProductId AND DescuentoProducto
        var item = Items.FirstOrDefault(x =>
            x.Product.Id == productId &&
            x.DescuentoProducto == descuentoProducto);

        if (item != null)
        {
            Items.Remove(item);
            await SaveAndNotify();
        }
    }

    public async Task Clear()
    {
        Items.Clear();
        _observaciones = "";
        await _store.RemoveAsync(KEY_OBSERVACIONES);
        await SaveAndNotify();
    }

    // ✅ MÉTODO CORREGIDO: Usa GetEffectiveCustomerNo() para determinar el cliente
    public async Task<Status> ProcessOrder(List<CartItem> carro, string direnvio, string usuario, string clienteParam)
    {
        var status = new Status();

        // ✅ CRÍTICO: Usar el cliente efectivo (CurrentCustomer o CurrentUser)
        string cliente = GetEffectiveCustomerNo();

        // Validar que tenemos código de cliente
        if (string.IsNullOrWhiteSpace(cliente))
        {
            return new Status
            {
                IsSuccess = false,
                Message = "No se pudo determinar el código de cliente para el pedido."
            };
        }

        // Obtener el descuento en factura del usuario/cliente actual
        var descuentoCabecera = (double)(_auth?.CurrentCustomer?.Desgeneral ??
                                         _auth?.CurrentUser?.DescuentoFactura ?? 0);

        // ✅ CAMBIO: Pasar Observaciones en lugar de ""
        var eprods = await _rest.PedidoVenta(
            carro,              // List<CartItem> - el carrito tal cual
            cliente,            // ✅ string cliente - código de cliente efectivo
            descuentoCabecera,  // double descuentoCabecera
            Observaciones,      // ✅ string observaciones - AHORA CON VALOR REAL
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

    // ✅ MÉTODO NUEVO: Determina el código de cliente efectivo
    private string GetEffectiveCustomerNo()
    {
        // Prioridad:
        // 1. Si hay CurrentCustomer (cliente seleccionado por Sales Team), usar ese
        // 2. Si no, usar el CustomerNo del CurrentUser (cliente normal o vendedor sin selección)
        return _auth?.CurrentCustomer?.CustNo ?? _auth?.CurrentUser?.CustomerNo ?? "";
    }

    // ✅ MÉTODO NUEVO: Guardar observaciones
    private async void SaveObservaciones()
    {
        await _store.SetAsync(KEY_OBSERVACIONES, _observaciones);
    }

    private async Task SaveAndNotify()
    {
        await _store.SetAsync(KEY, Items);
        Changed?.Invoke();
    }

    private void NotifyChanged() => Changed?.Invoke();
}
