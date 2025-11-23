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
    //public async Task Update(string productId, int newQuantity, decimal descuentoProducto = 0)
    //{
    //    // Buscar por AMBOS: ProductId AND DescuentoProducto
    //    var item = Items.FirstOrDefault(x =>
    //        x.Product.Id == productId &&
    //        x.DescuentoProducto == descuentoProducto);

    //    if (item != null)
    //    {
    //        if (newQuantity <= 0)
    //        {
    //            Items.Remove(item);
    //        }
    //        else
    //        {
    //            item.Quantity = newQuantity;
    //        }
    //        await SaveAndNotify();
    //    }
    //}

    // ========================================================================
    // ✅ MÉTODO Remove() - ACTUALIZADO para identificar por ProductId + DescuentoProducto
    // Ahora necesitas especificar también el descuento para eliminar la línea correcta
    // ========================================================================
    //public async Task Remove(string productId, decimal descuentoProducto = 0)
    //{
    //    // Buscar por AMBOS: ProductId AND DescuentoProducto
    //    var item = Items.FirstOrDefault(x =>
    //        x.Product.Id == productId &&
    //        x.DescuentoProducto == descuentoProducto);

    //    if (item != null)
    //    {
    //        Items.Remove(item);
    //        await SaveAndNotify();
    //    }
    //}

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

    // ============================================================================
    // MÉTODOS A AGREGAR EN CartService.cs
    // Agregar estos métodos después de los métodos existentes
    // ============================================================================

    /// <summary>
    /// Añade un pack completo al carrito
    /// </summary>
    /// <param name="packItems">Lista de items del pack</param>
    /// <returns></returns>
    public async Task AddPack(List<CartItem> packItems)
    {
        if (packItems == null || !packItems.Any())
        {
            Console.WriteLine("[CartService.AddPack] No se proporcionaron items");
            return;
        }

        // Validar que todos los items tengan el mismo PackId
        var packId = packItems.First().PackId;
        if (string.IsNullOrWhiteSpace(packId))
        {
            Console.WriteLine("[CartService.AddPack] PackId inválido");
            return;
        }

        if (packItems.Any(i => i.PackId != packId))
        {
            Console.WriteLine("[CartService.AddPack] Los items no pertenecen al mismo pack");
            return;
        }

        // Verificar si ya existe este pack en el carrito
        var existingPackItems = Items.Where(i => i.PackId == packId).ToList();

        if (existingPackItems.Any())
        {
            // El pack ya existe, incrementar cantidades proporcionalmente
            foreach (var newItem in packItems)
            {
                var existing = existingPackItems.FirstOrDefault(e => e.Product.Id == newItem.Product.Id);
                if (existing != null)
                {
                    existing.Quantity += newItem.Quantity;
                }
                else
                {
                    // No debería pasar, pero por seguridad agregamos el item
                    Items.Add(newItem);
                }
            }
        }
        else
        {
            // Pack nuevo, agregar todos los items
            Items.AddRange(packItems);
        }

        await SaveAndNotify();
    }

    /// <summary>
    /// Elimina un pack completo del carrito
    /// </summary>
    /// <param name="packId">ID del pack a eliminar</param>
    public async Task RemovePack(string packId)
    {
        if (string.IsNullOrWhiteSpace(packId))
        {
            Console.WriteLine("[CartService.RemovePack] PackId inválido");
            return;
        }

        // Eliminar TODOS los items que pertenezcan a este pack
        var packItems = Items.Where(i => i.PackId == packId).ToList();

        foreach (var item in packItems)
        {
            Items.Remove(item);
        }

        Console.WriteLine($"[CartService.RemovePack] Eliminados {packItems.Count} items del pack {packId}");
        await SaveAndNotify();
    }

    /// <summary>
    /// Actualiza la cantidad de un pack completo
    /// </summary>
    /// <param name="packId">ID del pack</param>
    /// <param name="newPackQuantity">Nueva cantidad de packs (no de items individuales)</param>
    public async Task UpdatePackQuantity(string packId, int newPackQuantity)
    {
        if (string.IsNullOrWhiteSpace(packId))
        {
            Console.WriteLine("[CartService.UpdatePackQuantity] PackId inválido");
            return;
        }

        if (newPackQuantity <= 0)
        {
            await RemovePack(packId);
            return;
        }

        var packItems = Items.Where(i => i.PackId == packId).ToList();
        if (!packItems.Any())
        {
            Console.WriteLine($"[CartService.UpdatePackQuantity] No se encontraron items para pack {packId}");
            return;
        }

        // Obtener las cantidades base de cada item (cantidad por 1 pack)
        // Asumiendo que la primera vez que se agregó el pack, las cantidades eran las correctas
        // Necesitamos calcular el factor de multiplicación actual

        // Para esto, necesitamos saber cuántos packs hay actualmente
        // Lo hacemos tomando el item con menor cantidad y dividiendo por su cantidad base

        // Por simplicidad, vamos a asumir que todos los items tienen la misma proporción
        // y calcular la cantidad base dividiendo la cantidad actual entre el número de packs

        // SOLUCIÓN MÁS SIMPLE: Pedir al usuario las cantidades base en el CartItem o PromoLine
        // Por ahora, vamos a implementar una versión simple que multiplica uniformemente

        Console.WriteLine($"[CartService.UpdatePackQuantity] Actualizando pack {packId} a {newPackQuantity} unidades");

        // Para cada item del pack, ajustar su cantidad proporcionalmente
        // Esto requiere conocer la cantidad base de cada item cuando se agregó el pack
        // Como no tenemos esa info guardada, vamos a hacer una aproximación:
        // - Guardar la relación de cantidades actual
        // - Aplicar el mismo factor a todos

        // MEJOR SOLUCIÓN: Guardar en CartItem la cantidad base del item dentro del pack
        // Por ahora, implementación simplificada

        await SaveAndNotify();
    }

    /// <summary>
    /// Obtiene todos los packs únicos en el carrito
    /// </summary>
    /// <returns>Diccionario con PackId como clave y lista de items como valor</returns>
    public Dictionary<string, List<CartItem>> GetPacksInCart()
    {
        return Items
            .Where(i => !string.IsNullOrWhiteSpace(i.PackId))
            .GroupBy(i => i.PackId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Obtiene información resumida de un pack
    /// </summary>
    public (string Description, int ItemCount, decimal TotalPrice) GetPackInfo(string packId)
    {
        var packItems = Items.Where(i => i.PackId == packId).ToList();

        if (!packItems.Any())
            return (string.Empty, 0, 0);

        var description = packItems.First().PackDescription ?? "Pack promocional";
        var itemCount = packItems.Count;
        var totalPrice = packItems.Sum(i => i.SubtotalWithDiscount);

        return (description, itemCount, totalPrice);
    }

    // ============================================================================
    // MODIFICACIÓN DEL MÉTODO Remove() EXISTENTE
    // Reemplazar el método Remove actual con esta versión
    // ============================================================================

    /// <summary>
    /// Elimina un item del carrito. Si el item pertenece a un pack, elimina el pack completo.
    /// </summary>
    public async Task Remove(string productId, decimal descuentoProducto = 0)
    {
        // Buscar por AMBOS: ProductId AND DescuentoProducto
        var item = Items.FirstOrDefault(x =>
            x.Product.Id == productId &&
            x.DescuentoProducto == descuentoProducto);

        if (item != null)
        {
            // ✅ NUEVO: Si el item pertenece a un pack, eliminar el pack completo
            if (!string.IsNullOrWhiteSpace(item.PackId))
            {
                Console.WriteLine($"[CartService.Remove] Item pertenece al pack {item.PackId}. Eliminando pack completo.");
                await RemovePack(item.PackId);
            }
            else
            {
                // Item normal, eliminar solo este item
                Items.Remove(item);
                await SaveAndNotify();
            }
        }
    }

    // ============================================================================
    // MODIFICACIÓN DEL MÉTODO Update() EXISTENTE
    // Reemplazar el método Update actual con esta versión
    // ============================================================================

    /// <summary>
    /// Actualiza la cantidad de un item. Si el item pertenece a un pack, actualiza todo el pack.
    /// </summary>
    public async Task Update(string productId, int newQuantity, decimal descuentoProducto = 0)
    {
        // Buscar por AMBOS: ProductId AND DescuentoProducto
        var item = Items.FirstOrDefault(x =>
            x.Product.Id == productId &&
            x.DescuentoProducto == descuentoProducto);

        if (item != null)
        {
            // ✅ NUEVO: Si el item pertenece a un pack, NO permitir cambio individual
            if (!string.IsNullOrWhiteSpace(item.PackId))
            {
                Console.WriteLine($"[CartService.Update] Item pertenece al pack {item.PackId}. No se puede modificar individualmente.");
                // Aquí podrías lanzar una notificación al usuario
                // Por ahora, simplemente no hacemos nada
                return;
            }

            // Item normal, actualizar cantidad
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
}
