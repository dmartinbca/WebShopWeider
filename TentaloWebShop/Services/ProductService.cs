using System.Net.Http.Json;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private List<Product>? _cache;

    public ProductService(HttpClient http) => _http = http;

    public async Task<List<Product>> GetAllAsync()
    {
        if (_cache is not null) return _cache;
        var list = await _http.GetFromJsonAsync<List<Product>>("data/products.json") ?? new();
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