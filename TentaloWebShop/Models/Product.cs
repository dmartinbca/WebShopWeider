namespace TentaloWebShop.Models;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public string ImageUrl { get; set; } = "/images/image.png";
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FamilySlug { get; set; } = "";      // p.ej. "better-nutrition"
    public string SubfamilySlug { get; set; } = "";   // p.ej. "proteinas"
}