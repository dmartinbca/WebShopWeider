namespace TentaloWebShop.Models;

// Modelos para el flujo de aprobación de pedidos de atletas

public class AthleteApproval
{
    public string Id { get; set; } = string.Empty;
    public int EntryNo { get; set; }
    public int BufferPedidoNo { get; set; }
    public string CustomerNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string SalespersonCode { get; set; } = string.Empty;
    public string SalespersonName { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal AthleteCreditUsed { get; set; }
    public decimal DiscountPercent { get; set; }
    public string ApprovalReason { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public DateTime? CreatedDateTime { get; set; }
    public DateTime? SentDateTime { get; set; }
    public DateTime? ResponseDateTime { get; set; }
    public string UserApp { get; set; } = string.Empty;
    public string ShipToCode { get; set; } = string.Empty;
}

public class AthleteApprovalLine
{
    public int LineNo { get; set; }
    public string ItemNo { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasureCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public decimal LineDiscount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAthleteCredit { get; set; }
}

public class ApprovalDetails
{
    public ApprovalCompanyInfo? CompanyInfo { get; set; }
    public AthleteApproval? Header { get; set; }
    public List<AthleteApprovalLine> Lines { get; set; } = new();
    public int TotalLines { get; set; }
}

public class ApprovalCompanyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Name2 { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Address2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VatRegistrationNo { get; set; } = string.Empty;
    public string LogoBase64 { get; set; } = string.Empty;
}

public class ApprovalActionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? OrderNo { get; set; }
}

/// <summary>
/// Resultado del endpoint checkAndProcessOrder
/// </summary>
public class CheckOrderResult
{
    public bool NeedsApproval { get; set; }
    public string? ApprovalId { get; set; }
    public string? OrderNo { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Wrapper para respuestas de BC API que vienen como { "value": "json_string" }
/// </summary>
public class ApprovalDetailsStringResponse
{
    public string Value { get; set; } = string.Empty;
}
