namespace TentaloWebShop.Models;

public class Invoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
}