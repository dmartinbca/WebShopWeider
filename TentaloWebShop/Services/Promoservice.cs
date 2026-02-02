using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

/// <summary>
/// Servicio para gestionar ofertas y promociones
/// </summary>
public class PromoService
{
    private readonly RestDataService _rest;
    private readonly AuthService _auth;
    private readonly ClientSelectionService _clientSelection;
    private readonly ProductService _productService;

    private List<PromoHeader>? _cache;
    private string? _lastCustomerNo;

    public PromoService(
        RestDataService rest,
        AuthService auth,
        ClientSelectionService clientSelection,
        ProductService productService)
    {
        _rest = rest;
        _auth = auth;
        _clientSelection = clientSelection;
        _productService = productService;

        // Suscribirse a cambios de cliente
        _auth.OnCustomerChanged += OnCustomerChanged;
        _clientSelection.OnClientChanged += OnClientSelectionChanged;
    }

    private async Task OnCustomerChanged()
    {
        Console.WriteLine("[PromoService.OnCustomerChanged] Limpiando caché");
        ClearCache();
        await Task.CompletedTask;
    }

    private void OnClientSelectionChanged()
    {
        Console.WriteLine("[PromoService.OnClientSelectionChanged] Limpiando caché");
        ClearCache();
    }

    /// <summary>
    /// Obtiene todas las promociones activas
    /// </summary>
    public async Task<List<PromoHeader>> GetAllPromosAsync()
    {
        string customerNo = GetEffectiveCustomerGroupNo();

        // Si cambió el cliente, invalidar caché
        if (_lastCustomerNo != customerNo)
        {
            _cache = null;
            _lastCustomerNo = customerNo;
        }

        if (_cache is not null) return _cache;

        var promos = await _rest.GetPromocionesAPICloud(customerNo);
        _cache = promos;

        return _cache;
    }

    /// <summary>
    /// Obtiene una promoción específica por su código
    /// </summary>
    public async Task<PromoHeader?> GetPromoByCodeAsync(string promoCode)
    {
        var all = await GetAllPromosAsync();
        return all.FirstOrDefault(p => p.Code.Equals(promoCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Convierte una promoción a un conjunto de CartItems listos para agregar al carrito
    /// </summary>
    /// <param name="promo">Promoción a convertir</param>
    /// <param name="packQuantity">Cantidad de packs a agregar</param>
    /// <returns>Lista de CartItems con la información del pack</returns>
    public async Task<List<CartItem>> ConvertPromoToCartItems(PromoHeader promo, int packQuantity = 1)
    {
        var cartItems = new List<CartItem>();
        var packId = Guid.NewGuid().ToString(); // ID único para este pack específico

        // Obtener todos los productos para poder mapearlos
        var allProducts = await _productService.GetAllAsync();

        // Procesar cada línea de la promoción
        foreach (var line in promo.Lines)
        {
            // Buscar el producto correspondiente
            var product = allProducts.FirstOrDefault(p =>
                p.Itemno.Equals(line.ItemNo, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                Console.WriteLine($"[PromoService] Producto no encontrado: {line.ItemNo}");
                continue;
            }

            // Crear el CartItem con información del pack
            var cartItem = new CartItem
            {
                Product = product,
                Quantity = line.Quantity * packQuantity, // Multiplicar por cantidad de packs
                DescuentoProducto = line.DiscountPercent,

                // ✅ Información del pack
                PackId = packId,
                PromoCode = promo.Code,
                PackLineType = line.LineType,
                PackDescription = promo.Description
            };

            cartItems.Add(cartItem);
        }

        return cartItems;
    }

    /// <summary>
    /// Convierte una selección N+1 del usuario a CartItems
    /// </summary>
    /// <param name="promo">Promoción N+1</param>
    /// <param name="selection">Selección del usuario (cantidades + regalo)</param>
    /// <returns>Lista de CartItems con la información del pack</returns>
    public async Task<List<CartItem>> ConvertNPlusOneToCartItems(PromoHeader promo, NPlusOneSelection selection)
    {
        var cartItems = new List<CartItem>();
        var packId = Guid.NewGuid().ToString();

        // Obtener todos los productos para poder mapearlos
        var allProducts = await _productService.GetAllAsync();

        // Procesar productos seleccionados (líneas de venta)
        foreach (var kvp in selection.SelectedQuantities)
        {
            if (kvp.Value <= 0) continue;

            var itemNo = kvp.Key;
            var quantity = kvp.Value;

            // Buscar el producto correspondiente
            var product = allProducts.FirstOrDefault(p =>
                p.Itemno.Equals(itemNo, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                Console.WriteLine($"[PromoService] Producto no encontrado: {itemNo}");
                continue;
            }

            // Buscar la línea de la promo para obtener el precio
            var promoLine = promo.Lines.FirstOrDefault(l =>
                l.ItemNo.Equals(itemNo, StringComparison.OrdinalIgnoreCase));

            var cartItem = new CartItem
            {
                Product = product,
                Quantity = quantity,
                DescuentoProducto = 0, // Sin descuento para las líneas de venta en N+1

                PackId = packId,
                PromoCode = promo.Code,
                PackLineType = "Venta",
                PackDescription = promo.Description
            };

            cartItems.Add(cartItem);
        }

        // Procesar el regalo (siempre 1 unidad, 100% descuento)
        if (!string.IsNullOrEmpty(selection.GiftItemNo))
        {
            var giftProduct = allProducts.FirstOrDefault(p =>
                p.Itemno.Equals(selection.GiftItemNo, StringComparison.OrdinalIgnoreCase));

            if (giftProduct != null)
            {
                var giftItem = new CartItem
                {
                    Product = giftProduct,
                    Quantity = 1,
                    DescuentoProducto = 100, // 100% descuento = GRATIS

                    PackId = packId,
                    PromoCode = promo.Code,
                    PackLineType = "Regalo",
                    PackDescription = promo.Description
                };

                cartItems.Add(giftItem);
            }
        }

        return cartItems;
    }

    /// <summary>
    /// Valida si una promoción puede ser añadida (tiene todos los productos disponibles)
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage)> ValidatePromo(PromoHeader promo)
    {
        var allProducts = await _productService.GetAllAsync();

        foreach (var line in promo.Lines)
        {
            var product = allProducts.FirstOrDefault(p =>
                p.Itemno.Equals(line.ItemNo, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                return (false, $"El producto {line.Description} no está disponible");
            }

            // Aquí podrías agregar validaciones adicionales:
            // - Stock disponible
            // - Precio mínimo
            // - Etc.
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Valida una selección N+1 del usuario
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateNPlusOneSelection(PromoHeader promo, NPlusOneSelection selection)
    {
        if (!promo.IsNPlusOne)
        {
            return (false, "Esta promoción no es de tipo N+1");
        }

        if (selection.TotalSelected != promo.QuantityN)
        {
            return (false, $"Debes seleccionar exactamente {promo.QuantityN} unidades. Has seleccionado {selection.TotalSelected}.");
        }

        if (string.IsNullOrEmpty(selection.GiftItemNo))
        {
            return (false, "Debes seleccionar un producto como regalo");
        }

        // Verificar que el regalo está entre los productos disponibles
        var validItemNos = promo.Lines.Select(l => l.ItemNo).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!validItemNos.Contains(selection.GiftItemNo))
        {
            return (false, "El producto de regalo seleccionado no es válido");
        }

        // Verificar que los productos seleccionados son válidos
        foreach (var itemNo in selection.SelectedQuantities.Keys)
        {
            if (!validItemNos.Contains(itemNo))
            {
                return (false, $"El producto {itemNo} no es válido para esta promoción");
            }
        }

        return (true, string.Empty);
    }

    private string GetEffectiveCustomerNo()
    {
        // Si es Sales Team y hay un cliente seleccionado, usar ese
        if (_auth.CurrentUser?.Tipo == "Sales Team" && _clientSelection.SelectedClient != null)
        {
            return _clientSelection.SelectedClient.CustNo;
        }

        // Si es Customer o Sales Team sin selección, usar el propio usuario
        return _auth.CurrentUser?.CustomerNo ?? "";
    }
    private string GetEffectiveCustomerGroupNo()
    {
        // Si es Sales Team y hay un cliente seleccionado, usar ese
        //if (_auth.CurrentUser?.Tipo == "Customer" && _clientSelection.SelectedClient != null)
        //{
        //    return _clientSelection.SelectedClient.Code;
        //}

        // Si es Customer o Sales Team sin selección, usar el propio usuario
        return _auth.CurrentCustomer.Code;
    }



    public void ClearCache()
    {
        _cache = null;
        _lastCustomerNo = null;
    }
}