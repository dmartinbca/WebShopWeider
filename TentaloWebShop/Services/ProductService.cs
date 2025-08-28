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
          
        var eprods = await _rest.GetProductosAPICloud("","");
        if (eprods!=null)
        {
            foreach (var p in eprods)
            {
                try
                {
                    list.Add(new Product
                    {

                        Name = p.Description ?? "",
                        Slug = p.Description.Replace(" ", "-"),
                        Description = p.ingredientes ?? "",
                        PriceFrom = Convert.ToDecimal(p.ActualPrice),
                        PriceTo = Convert.ToDecimal(p.ActualPrice),
                        ImageUrl = p.ImageUrl,
                        FamilySlug = (p.FamiliaN.Replace(" ", "-")) ?? "",
                        SubfamilySlug = string.IsNullOrEmpty(p.SubFamilia) ? "" : p.SubFamilia.Replace(" ", "-")
                    });
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"[GetAllAsync] {ex.Message}");
                }
                
                
            }
        }
     
        _cache = list;
        return list;
    }
    public async Task<IEnumerable<Product>> GetByFamilyAsync(string familySlug)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.FamilySlug.Equals(familySlug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Product>> GetBySubfamilyAsync(string familySlug, string subSlug)
    {
        var all = await GetAllAsync();
        return all.Where(p =>
            p.FamilySlug.Equals(familySlug, StringComparison.OrdinalIgnoreCase) &&
            p.SubfamilySlug.Equals(subSlug, StringComparison.OrdinalIgnoreCase));
    }

}