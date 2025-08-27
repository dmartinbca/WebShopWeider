using TentaloWebShop.Models;
 

namespace TentaloWebShop.Services;

public class CategoryService
{
    // ⚠️ Rellena aquí tus familias y subfamilias reales
    private readonly List<Category> _families = new()
    {
        new Category {
            Name = "Better Nutrition", Slug = "better-nutrition",
            Subs = new()
            {
                new Subcategory { Name="Proteínas",   Slug="proteinas" },
                new Subcategory { Name="Aminoácidos", Slug="aminoacidos" },
                new Subcategory { Name="Gainers",     Slug="gainers" },
            }
        },
        new Category {
            Name = "Barritas y Snacks", Slug = "barritas-snacks",
            Subs = new()
            {
                new Subcategory { Name="Barritas",    Slug="barritas" },
                new Subcategory { Name="Galletas",    Slug="galletas" },
            }
        },
        new Category {
            Name = "Energéticos y Pre-entrenos", Slug = "energeticos-preentrenos",
            Subs = new()
            {
                new Subcategory { Name="Pre-entreno", Slug="preentreno" },
                new Subcategory { Name="Geles",       Slug="geles" },
            }
        },
        new Category {
            Name = "Creatinas", Slug = "creatinas",
            Subs = new()
            {
                new Subcategory { Name="Monohidrato", Slug="monohidrato" },
                new Subcategory { Name="Otros",       Slug="otros" },
            }
        },
    };

    public List<Category> GetFamilies() => _families;
    public Category? GetFamily(string familySlug)
        => _families.FirstOrDefault(f => f.Slug.Equals(familySlug, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Subcategory> GetSubs(string familySlug)
        => GetFamily(familySlug)?.Subs ?? Enumerable.Empty<Subcategory>();

    public Subcategory? GetSub(string familySlug, string subSlug)
        => GetFamily(familySlug)?.Subs.FirstOrDefault(s => s.Slug.Equals(subSlug, StringComparison.OrdinalIgnoreCase));
}
