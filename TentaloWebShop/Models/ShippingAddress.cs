namespace TentaloWebShop.Models;

public class ShippingAddress
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public override string ToString()
    {
        return $"{Description} - {Address}, {City}";
    }
}
