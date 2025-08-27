namespace TentaloWebShop.Models;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<CartItem> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Product.PriceFrom * l.Quantity);
}