using TentaloWebShop.Models;
 

namespace TentaloWebShop.Services;

public class CategoryService
{
    private readonly RestDataService _rest;
    public CategoryService(RestDataService rest) => _rest = rest;
    private   List<Category> _families = new List<Category>();

    // Slugs de familias marcadas como Material Promocional
    private HashSet<string> _materialPromocionalSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<string> MaterialPromocionalSlugs => _materialPromocionalSlugs;

    public async Task<List<Category>> GetFamilies()
    {
        var listFam = new List<Category>();

        HttpClient httpClient = new HttpClient();

        var efam = await _rest.GetFamiliasAPICloud();

        if (efam != null)
        {
            _materialPromocionalSlugs.Clear();
            foreach (var fam in efam)
            {
                var listSubFam = new List<Subcategory>();
                foreach (var sub in fam.subfamlines)
                {
                    listSubFam.Add(new Subcategory { Name = sub.No, Slug = sub.No.Replace(" ", "") });
                }
                listFam.Add(new Category { Name = fam.Famlia, Slug = fam.Famlia.Replace(" ", ""), Subs=listSubFam });

                // Registrar familias de material promocional
                if (fam.MaterialPromocional)
                {
                    _materialPromocionalSlugs.Add(fam.Famlia.Replace(" ", ""));
                }
            }
            _families=listFam;
            return listFam;
        }
        else
        {
            return listFam;
        }
    }

    public bool IsMaterialPromocional(string familySlug)
    {
        if (string.IsNullOrEmpty(familySlug)) return false;
        // También incluir MATERIALPROM como slug fijo por compatibilidad
        return _materialPromocionalSlugs.Contains(familySlug)
            || familySlug.Equals("MATERIALPROM", StringComparison.OrdinalIgnoreCase);
    }

    public Category? GetFamily(string familySlug)
        => _families.FirstOrDefault(f => f.Slug.Equals(familySlug, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Subcategory> GetSubs(string familySlug)
        => GetFamily(familySlug)?.Subs ?? Enumerable.Empty<Subcategory>();

    public Subcategory? GetSub(string familySlug, string subSlug)
        => GetFamily(familySlug)?.Subs.FirstOrDefault(s => s.Slug.Equals(subSlug, StringComparison.OrdinalIgnoreCase));
}
