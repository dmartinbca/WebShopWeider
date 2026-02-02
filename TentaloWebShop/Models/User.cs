namespace TentaloWebShop.Models;

public class User
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";

    public string Token { get; set; } = "";
    public string Tipo { get; set; } = "";
    public bool EsMaster { get; set; }= false;

    public string salesCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal EBalance { get; set; } = 0;

    public string CustomerNo { get; set; } = "";

    public string VatBusPostingGroup { get; set; } = "";

    public decimal DescuentoFactura { get; set; } = 0;
    public decimal DescuentoPP {  get; set; } = 0;

    public string IdiomaPais { get; set; } = "";
    public List<CustomerAddres> CustomerAddres { get; set; }

    public string TipoClienteWebShop { get; set; } = "Normal";
    public decimal PctDescMaterialProm { get; set; }
    public decimal CreditoAnualAtleta { get; set; }
    public decimal CreditoConsumidoAtleta { get; set; }

}