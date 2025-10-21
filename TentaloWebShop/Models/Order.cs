using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;
namespace TentaloWebShop.Models;

public class PedidosNAVJson
{

    [JsonProperty("@odata.context")]
    public string OdataContext { get; set; }

    [JsonProperty("value")]
    public IList<OrderNAVCabecera> Value { get; set; }
}
public class OrderNAVCabecera : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Document_Type")]
    public string Document_Type { get; set; }

    [JsonPropertyName("Buy_from_Vendor_No")]
    public string Buy_from_Vendor_No { get; set; }

    [JsonPropertyName("Buy_from_Vendor_Name")]
    public string Buy_from_Vendor_Name { get; set; }

    [JsonPropertyName("Sell_to_Customer_No")]
    public string Sell_to_Customer_No { get; set; }

    [JsonPropertyName("Sell_to_Customer_Name")]
    public string Sell_to_Customer_Name { get; set; }

    [JsonPropertyName("Sell_to_Address")]
    public string Sell_to_Address { get; set; }

    [JsonPropertyName("Sell_to_County")]
    public string Sell_to_County { get; set; }

    [JsonPropertyName("Sell_to_Post_Code")]
    public string Sell_to_Post_Code { get; set; }

    [JsonPropertyName("Sell_to_City")]
    public string Sell_to_City { get; set; }

    [JsonPropertyName("No")]
    public string No { get; set; }

    [JsonPropertyName("Order_Date")]
    public string Order_Date { get; set; }

    [JsonPropertyName("Posting_Date")]
    public string Posting_Date { get; set; }

    [JsonPropertyName("Expected_Receipt_Date")]
    public string Expected_Receipt_Date { get; set; }

    [JsonPropertyName("Location_Code")]
    public string Location_Code { get; set; }

    [JsonPropertyName("Usuario_App")]
    public string Usuario_App { get; set; }

    [JsonPropertyName("Token")]
    public string Token { get; set; }

    [JsonPropertyName("Web_Order")]
    public bool Web_Order { get; set; }


    [JsonPropertyName("Invoice_Discount_Value")]
    public double Invoice_Discount_Value { get; set; }


    double amount;
    [JsonPropertyName("Amount")]
    public double Amount
    {
        get => amount;
        set
        {
            if (amount == value) return;
            amount = value;
            NotifyPropertyChanged("Amount");
        }
    }


    double amountincludingvat;
    [JsonPropertyName("Amount_Including_VAT")]

    public double Amount_Including_VAT
    {
        get => amountincludingvat;
        set
        {
            amountincludingvat = value;
            NotifyPropertyChanged("Amount_Including_VAT");
            VAT_Amount = Amount_Including_VAT - Amount;
        }
    }

    double amountvat;


    public double VAT_Amount
    {
        get => amountvat;
        set
        {
            amountvat = value;
            NotifyPropertyChanged("VAT_Amount");
        }
    }


    [JsonPropertyName("lines")]
    public IList<OrderNAVLineas> Lines { get; set; }

    [JsonPropertyName("saleInvoicelines")]
    public IList<OrderNAVLineas> LineasFactura { get; set; }

    [JsonPropertyName("saleShipmmentlines")]
    public IList<OrderNAVLineas> LineasAlbaran { get; set; }

    [JsonPropertyName("salelines")]
    public IList<OrderNAVLineas> LineasVenta { get; set; }

    [JsonPropertyName("purchaselines")]
    public IList<OrderNAVLineas> Lineas { get; set; }

    private bool ocupadoboton;
    public bool OcupadoBoton
    {
        get => ocupadoboton;
        set
        {
            if (value == ocupadoboton) return;
            ocupadoboton = value;
            NotifyPropertyChanged("OcupadoBoton");
        }
    }

    private bool ocupadobotonAlb;
    public bool OcupadoBotonAlb
    {
        get => ocupadobotonAlb;
        set
        {
            if (value == ocupadobotonAlb) return;
            ocupadobotonAlb = value;
            NotifyPropertyChanged("OcupadoBotonAlb");
        }
    }

    private string vendor_Shipment_No;

    [JsonPropertyName("Vendor_Shipment_No")]
    public string Vendor_Shipment_No
    {
        get => vendor_Shipment_No;
        set
        {
            if (value == vendor_Shipment_No) return;
            vendor_Shipment_No = value;
            NotifyPropertyChanged("Vendor_Shipment_No");
        }
    }


    [JsonPropertyName("Status")]
    public string Status { get; set; }



    [JsonPropertyName("Tipo")]
    public string Tipo { get; set; }


    private double descuento;
    [JsonPropertyName("VAT_Base_Discount_Percent")]
    public double VAT_Base_Discount_Percent
    {
        get => descuento;
        set
        {
            if (value == descuento) return;
            descuento = value;
            NotifyPropertyChanged("VAT_Base_Discount_Percent");

        }
    }

    [JsonPropertyName("Tiene_Albaran_Adjunto")]
    public bool Tienealbaran { get; set; }


    private string nombreproveedor;
    [JsonPropertyName("NombreProveedor")]
    public string NombreProveedor
    {
        get => nombreproveedor;
        set
        {
            nombreproveedor = value;
            NotifyPropertyChanged("NombreProveedor");
        }
    }
}
public class OrderNAVLineas : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    [JsonPropertyName("Document_Id")]
    public string Document_Id { get; set; }

    [JsonPropertyName("Document_Type")]
    public string Document_Type { get; set; }

    [JsonPropertyName("Buy_from_Vendor_No")]
    public string Buy_from_Vendor_No { get; set; }

    [JsonPropertyName("Sell_to_Customer_No")]
    public string Sell_to_Customer_No { get; set; }


    [JsonPropertyName("Document_No")]
    public string Document_No { get; set; }

    [JsonPropertyName("Line_No")]
    public int Line_No { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("No")]
    public string No { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }

    [JsonPropertyName("Location_Code")]
    public string Location_Code { get; set; }

    [JsonPropertyName("Unit_of_Measure")]
    public string Unit_of_Measure { get; set; }

    [JsonPropertyName("Quantity")]
    public double Quantity { get; set; }

    [JsonPropertyName("Unit_Price")]
    public double Unit_Price { get; set; }

    private double direct_Unit_Cost;

    [JsonPropertyName("Direct_Unit_Cost")]
    public double Direct_Unit_Cost
    {

        get => direct_Unit_Cost;
        set
        {

            direct_Unit_Cost = value;
          
            NotifyPropertyChanged("Direct_Unit_Cost");
        }
    }

    double amount;
    [JsonPropertyName("Amount")]
    public double Amount { get; set; }
    
    private double opacidad;
    public double Opacidad
    {
        get => opacidad;
        set
        {
            opacidad = value;
            NotifyPropertyChanged("Opacidad");
        }
    }




    private double qty_to_Receive;
    [JsonPropertyName("Qty_to_Receive")]
    public double Qty_to_Receive
    {
        get => qty_to_Receive;
        set
        {

            qty_to_Receive = value;
           // Amount = (Direct_Unit_Cost * Qty_to_Receive) - (((Direct_Unit_Cost * Qty_to_Receive) * Line_Discount_Percent) / 100);

            NotifyPropertyChanged("Qty_to_Receive");

        }
    }
    [JsonPropertyName("Ship_Unit_of_Measure")]
    public string Ship_Unit_of_Measure { get; set; }

    [JsonPropertyName("VAT_Percent")]
    public double VAT_Percent { get; set; }

    [JsonPropertyName("Qty_Per_Package")]
    public double Qty_Per_Package { get; set; }

    [JsonPropertyName("Quantity_Received")]
    public double Quantity_Received { get; set; }

    private double descuentolinea;

    [JsonPropertyName("Line_Discount_Percent")]
    public double Line_Discount_Percent
    {
        get => descuentolinea;
        set
        {
            descuentolinea = value;
        //    Amount = (Direct_Unit_Cost * Qty_to_Receive) - (((Direct_Unit_Cost * Qty_to_Receive) * Line_Discount_Percent) / 100);     
            NotifyPropertyChanged("Line_Discount_Percent");
        }
    }

   
}