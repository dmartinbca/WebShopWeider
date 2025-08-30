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
    public double inventario100 { get; set; }
    public double inventarioNo100 { get; set; }
    public string EAN13 { get; set; }
    public double TipoIva { get; set; }
    public bool Promotion { get; set; }

    public double Descuento { get; set; }
     public int Presentation_Qty { get; set; }
    public double Presentation_Price { get; set; }
    public string Presentation_Unit { get; set; } = "";
    public string FamilySlug { get; set; } = "";      // p.ej. "better-nutrition"
    public string SubfamilySlug { get; set; } = "";   // p.ej. "proteinas"
}