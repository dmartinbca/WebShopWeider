using TentaloWebShop.Models;
 

namespace TentaloWebShop.Services;

public class CategoryService
{
    private readonly RestDataService _rest;
    public CategoryService(RestDataService rest) => _rest = rest;
    // ⚠️ Rellena aquí tus familias y subfamilias reales
    private   List<Category> _families = new List<Category>();


    //public List<Category> GetFamilies() => _families;
    public async Task<List<Category>> GetFamilies()
    {
        var listFam = new List<Category>();
       
        HttpClient httpClient = new HttpClient();
       
        var efam = await _rest.GetFamiliasAPICloud();
     
        if (efam != null)
        {
            foreach (var fam in efam)
            {
                var listSubFam = new List<Subcategory>();
                foreach (var sub in fam.subfamlines)
                {
                    listSubFam.Add(new Subcategory { Name = sub.No, Slug = sub.No.Replace(" ", "") });
                }
                listFam.Add(new Category { Name = fam.Famlia, Slug = fam.Famlia.Replace(" ", ""), Subs=listSubFam });
            }
            _families=listFam;
            return listFam;
        }
        else
        {
            return listFam;
        }
    }

    public Category? GetFamily(string familySlug)
        => _families.FirstOrDefault(f => f.Slug.Equals(familySlug, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Subcategory> GetSubs(string familySlug)
        => GetFamily(familySlug)?.Subs ?? Enumerable.Empty<Subcategory>();

    public Subcategory? GetSub(string familySlug, string subSlug)
        => GetFamily(familySlug)?.Subs.FirstOrDefault(s => s.Slug.Equals(subSlug, StringComparison.OrdinalIgnoreCase));
}
