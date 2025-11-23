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

    // ✅ NUEVO: Información del pack al que pertenece este item
    /// <summary>
    /// ID único del pack al que pertenece este item.
    /// Si es null, el item NO pertenece a ningún pack.
    /// </summary>
    public string? PackId { get; set; }

    /// <summary>
    /// Código de promoción del pack
    /// </summary>
    public string? PromoCode { get; set; }

    /// <summary>
    /// Tipo de línea dentro del pack: "Venta" o "Regalo"
    /// </summary>
    public string? PackLineType { get; set; }

    /// <summary>
    /// Descripción del pack al que pertenece
    /// </summary>
    public string? PackDescription { get; set; }

    /// <summary>
    /// Indica si este item es parte de un pack promocional
    /// </summary>
    public bool IsPartOfPack => !string.IsNullOrWhiteSpace(PackId);

    /// <summary>
    /// Indica si es un item de regalo dentro del pack
    /// </summary>
    public bool IsPackGift => IsPartOfPack && PackLineType == "Regalo";

    /// <summary>
    /// Indica si es un item de venta dentro del pack
    /// </summary>
    public bool IsPackSale => IsPartOfPack && PackLineType == "Venta";

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