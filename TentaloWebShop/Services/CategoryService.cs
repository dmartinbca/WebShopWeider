using TentaloWebShop.Models;
 

namespace TentaloWebShop.Services;

public class CategoryService
{
    private readonly RestDataService _rest;
    private readonly LocalizationService _localization;
    public CategoryService(RestDataService rest, LocalizationService localization)
    {
        _rest = rest;
        _localization = localization;
        _localization.OnLanguageChanged += OnLanguageChanged;
    }
    private List<Category> _families = new List<Category>();
    private List<Category> _rawFamilies = new List<Category>();

    // Slugs de familias marcadas como Material Promocional
    private HashSet<string> _materialPromocionalSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<string> MaterialPromocionalSlugs => _materialPromocionalSlugs;

    public async Task<List<Category>> GetFamilies()
    {
        // Si ya tenemos datos raw, solo re-traducir
        if (_rawFamilies.Any())
        {
            _families = TranslateFamilies(_rawFamilies);
            return _families;
        }

        var listFam = new List<Category>();

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
                listFam.Add(new Category { Name = fam.Famlia, Slug = fam.Famlia.Replace(" ", ""), Subs = listSubFam });

                // Registrar familias de material promocional
                if (fam.MaterialPromocional)
                {
                    _materialPromocionalSlugs.Add(fam.Famlia.Replace(" ", ""));
                }
            }
            _rawFamilies = listFam;
            _families = TranslateFamilies(listFam);
            return _families;
        }
        else
        {
            return listFam;
        }
    }

    private List<Category> TranslateFamilies(List<Category> raw)
    {
        return raw.Select(f => new Category
        {
            Name = _localization.TranslateFamily(f.Name),
            Slug = f.Slug,
            Subs = f.Subs.Select(s => new Subcategory
            {
                Name = _localization.TranslateSubfamily(s.Name),
                Slug = s.Slug
            }).ToList()
        }).ToList();
    }

    private void OnLanguageChanged()
    {
        if (_rawFamilies.Any())
        {
            _families = TranslateFamilies(_rawFamilies);
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
