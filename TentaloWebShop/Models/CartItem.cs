namespace TentaloWebShop.Models;

public class CartItem
{
    public Product Product { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Descuento aplicado a este producto específico (porcentaje)
    /// Ej: 10 = 10% de descuento sobre el precio unitario
    /// </summary>
    public decimal DescuentoProducto { get; set; } = 0;

    /// <summary>
    /// Calcula el precio unitario con descuento aplicado
    /// </summary>
    public decimal PriceWithDiscount
    {
        get
        {
            if (DescuentoProducto <= 0)
                return Product?.PriceFrom ?? 0;

            decimal precioUnitario = Product?.PriceFrom ?? 0;
            decimal descuentoAbsoluto = precioUnitario * (DescuentoProducto / 100);
            return precioUnitario - descuentoAbsoluto;
        }
    }

    /// <summary>
    /// Calcula el subtotal de la línea con descuento aplicado
    /// </summary>
    public decimal SubtotalWithDiscount => PriceWithDiscount * Quantity;
}