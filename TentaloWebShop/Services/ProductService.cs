using System.Net.Http.Json;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private List<Product>? _cache;
    private readonly RestDataService _rest;
    private readonly AuthService _auth;
    private readonly ClientSelectionService _clientSelection;
    private string? _lastCustomerNo; // Para detectar cambios de cliente

    public ProductService(RestDataService rest, AuthService auth, ClientSelectionService clientSelection)
    {
        _rest = rest;
        _auth = auth;
        _clientSelection = clientSelection;
        _auth.OnCustomerChanged += OnCustomerChanged;
        _clientSelection.OnClientChanged += OnCustomerChanged;
    }
    private void OnCustomerChanged()
    {
        // Limpiar cach� cuando cambia el cliente
        ClearCache();
    }
    public async Task<List<Product>> GetAllAsync()
    {
        // Determinar qu� n�mero de cliente usar
        string customerNo = GetEffectiveCustomerNo();

        // Si cambi� el cliente, invalidar cach�
        if (_lastCustomerNo != customerNo)
        {
            _cache = null;
            _lastCustomerNo = customerNo;
        }

        if (_cache is not null) return _cache;

        var list = new List<Product>();
        var eprods = await _rest.GetProductosAPICloud("", "", customerNo);

        if (eprods != null)
        {
            foreach (var p in eprods)
            {
                try
                {
                    list.Add(new Product
                    {
                        Name = p.Description ?? "",
                        Slug = p.Description.Replace(" ", ""),
                        Description = p.ingredientes ?? "",
                        PriceFrom = Convert.ToDecimal(p.Presentation_Price),
                        PriceTo = Convert.ToDecimal(p.ActualPrice),
                        TipoIva = p.TipoIva,
                        inventario100 = p.inventario100,
                        inventarioNo100 = p.inventarioNo100,
                        Itemno = p.No ?? "",
                        UnitofMeasure = p.UnitofMeasureCode ?? "",
                        ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "/images/image.png" : p.ImageUrl,
                        Presentation_Price = p.Presentation_Price,
                        Presentation_Qty = Convert.ToInt32(p.Presentation_Qty),
                        Presentation_Unit = p.Present_Unit ?? "",
                        FamilySlug = (p.FamiliaN.Replace(" ", "")) ?? "",
                        SubfamilySlug = string.IsNullOrEmpty(p.SubFamilia) ? "" : p.SubFamilia.Replace(" ", ""),

                        DescProducto = p.descripcion_Producto,
                        // NUEVOS CAMPOS MAPEADOS
                        Promotion = p.Promotion,
                        EsPack = p.Es_Pack,
                        EsNovedad = p.Marcar_como_Novedad
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetAllAsync] {ex.Message}");
                }
            }
        }

        _cache = list;
        return list;
    }

    public async Task<List<Product>> GetByFamilyAsync(string familySlug)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.FamilySlug.Equals(familySlug, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<Product>> GetBySubfamilyAsync(string familySlug, string subSlug)
    {
        var all = await GetAllAsync();
        return all.Where(p =>
            p.FamilySlug == familySlug &&
            p.SubfamilySlug == subSlug).ToList();
    }

    // M�todo auxiliar para determinar el n�mero de cliente efectivo
    private string GetEffectiveCustomerNo()
    {
        // Si es Sales Team y hay un cliente seleccionado, usar ese
        if (_auth.CurrentUser?.Tipo == "Sales Team" && _clientSelection.SelectedClient != null)
        {
            return _clientSelection.SelectedClient.CustNo;
        }

        // Si es Customer o Sales Team sin selecci�n, usar el propio usuario
        return _auth.CurrentUser?.CustomerNo ?? "";
    }

    // M�todo p�blico para forzar la recarga del cat�logo
    public void ClearCache()
    {
        _cache = null;
        _lastCustomerNo = null;
    }

    // NUEVOS M�TODOS PARA FILTROS ESPEC�FICOS

    /// <summary>
    /// Obtiene todos los productos marcados como promoci�n
    /// </summary>
    public async Task<List<Product>> GetPromotionsAsync()
    {
        var all = await GetAllAsync();
        return all.Where(p => p.Promotion).ToList();
    }

    /// <summary>
    /// Obtiene todos los productos que son packs
    /// </summary>
    public async Task<List<Product>> GetPacksAsync()
    {
        var all = await GetAllAsync();
        return all.Where(p => p.EsPack).ToList();
    }

    /// <summary>
    /// Obtiene todos los productos marcados como novedad
    /// </summary>
    public async Task<List<Product>> GetNovedadesAsync()
    {
        var all = await GetAllAsync();
        return all.Where(p => p.EsNovedad).ToList();
    }
}
