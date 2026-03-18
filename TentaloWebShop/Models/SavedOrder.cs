namespace TentaloWebShop.Models;

public class SavedOrder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public string CustomerNo { get; set; } = "";
    public List<CartItem> Items { get; set; } = new();
    public string Observaciones { get; set; } = "";
    public int TotalItems => Items.Sum(i => i.Quantity);
    public decimal EstimatedTotal => Items.Sum(i => i.SubtotalWithDiscount);
}
