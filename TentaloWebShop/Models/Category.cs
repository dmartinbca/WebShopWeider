namespace TentaloWebShop.Models;

public class Category
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<Subcategory> Subs { get; set; } = new();
}

public class Subcategory
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}