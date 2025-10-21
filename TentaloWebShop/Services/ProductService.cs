using System.Net.Http.Json;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private List<Product>? _cache;
    private readonly RestDataService _rest;

    public ProductService(RestDataService rest) => _rest = rest;

    public async Task<List<Product>> GetAllAsync()
    {
        if (_cache is not null) return _cache;
        var list = new List<Product>();

        var eprods = await _rest.GetProductosAPICloud("", "");
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
                        DescProducto=p.descripcion_Producto,
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

    // NUEVOS MÉTODOS PARA FILTROS ESPECÍFICOS

    /// <summary>
    /// Obtiene todos los productos marcados como promoción
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