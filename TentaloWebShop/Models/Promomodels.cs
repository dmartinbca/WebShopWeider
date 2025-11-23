using System.Text.Json.Serialization;

namespace TentaloWebShop.Models;

/// <summary>
/// Cabecera de una promoción/oferta con sus líneas de detalle
/// </summary>
public class PromoHeader
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("imageBase64")]
    public string ImageBase64 { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; set; } = string.Empty;

    [JsonPropertyName("salesOfferPackLines")]
    public List<PromoLine> Lines { get; set; } = new();

    // Propiedades calculadas
    public bool IsValid => Active ;

    public string ImageUrl => string.IsNullOrWhiteSpace(ImageBase64)
        ? "/images/image.png"
        : $"data:image/jpeg;base64,{ImageBase64}";

    /// <summary>
    /// Obtiene las líneas de tipo "Venta" (productos que se deben comprar)
    /// </summary>
    public List<PromoLine> SaleLines => Lines.Where(l => l.LineType == "Venta").ToList();

    /// <summary>
    /// Obtiene las líneas de tipo "Regalo" (productos promocionales)
    /// </summary>
    public List<PromoLine> GiftLines => Lines.Where(l => l.LineType == "Regalo").ToList();

    /// <summary>
    /// Calcula el precio total del pack (líneas de venta)
    /// </summary>
    public decimal TotalPrice => SaleLines.Sum(l => l.UnitPrice * l.Quantity);

    /// <summary>
    /// Calcula el ahorro total del pack (descuentos en líneas de regalo)
    /// </summary>
    public decimal TotalSavings => GiftLines.Sum(l => l.DiscountAmount * l.Quantity);

    /// <summary>
    /// Precio final del pack
    /// </summary>
    public decimal FinalPrice => TotalPrice - TotalSavings;
}

/// <summary>
/// Línea de detalle de una promoción
/// </summary>
public class PromoLine
{
    [JsonPropertyName("lineNo")]
    public int LineNo { get; set; }

    [JsonPropertyName("promoCode")]
    public string PromoCode { get; set; } = string.Empty;

    [JsonPropertyName("lineType")]
    public string LineType { get; set; } = string.Empty; // "Venta" o "Regalo"

    [JsonPropertyName("itemNo")]
    public string ItemNo { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("discountPercent")]
    public decimal DiscountPercent { get; set; }

    [JsonPropertyName("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [JsonPropertyName("unitOfMeasure")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Propiedades calculadas
    public decimal FinalPrice => UnitPrice - DiscountAmount;

    public decimal LineTotal => FinalPrice * Quantity;

    public bool IsSale => LineType == "Venta";

    public bool IsGift => LineType == "Regalo";
}

/// <summary>
/// Respuesta JSON del API de Business Central para promociones
/// </summary>
public class PromoHeaderJson
{
    [JsonPropertyName("value")]
    public List<PromoHeader> Value { get; set; } = new();
}

/// <summary>
/// Información de un pack en el carrito
/// </summary>
public class CartPackInfo
{
    /// <summary>
    /// Identificador único del pack en el carrito
    /// </summary>
    public string PackId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Código de la promoción de Business Central
    /// </summary>
    public string PromoCode { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del pack
    /// </summary>
    public string PackDescription { get; set; } = string.Empty;

    /// <summary>
    /// Imagen del pack en Base64
    /// </summary>
    public string ImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Lista de IDs de productos que pertenecen a este pack
    /// </summary>
    public List<string> ProductIds { get; set; } = new();

    /// <summary>
    /// Cantidad de packs (por si se añade el mismo pack múltiples veces)
    /// </summary>
    public int PackQuantity { get; set; } = 1;
}