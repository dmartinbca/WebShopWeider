using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TentaloWebShop.Models
{
    public class NavUser
    {

        [JsonPropertyName("eMail")]
        public string EMail { get; set; }

        [JsonPropertyName("appPass")]
        public string AppPass { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("phoneNo")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("auxiliaryIndex1")]
        public string AuxiliaryIndex1 { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("master")]
        public bool Master { get; set; }

        [JsonPropertyName("restaurante")]
        public string Restaurante { get; set; }

        [JsonPropertyName("encendido")]
        public bool Encendido { get; set; }

        [JsonPropertyName("invitar")]
        public bool Invitar { get; set; }

        [JsonPropertyName("cancelar")]
        public bool Cancelar { get; set; }

        [JsonPropertyName("modificar")]
        public bool Modificar { get; set; }

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("ultimoAcceso")]
        public string UltimoAcceso { get; set; }

        [JsonPropertyName("almacen")]
        public string Almacen { get; set; }

        [JsonPropertyName("esMaster")]
        public bool EsMaster { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("imagen")]
        public string Imagen { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("empresa")]
        public string Empresa { get; set; }


        [JsonPropertyName("pedidos")]
        public bool Pedidos { get; set; }

        [JsonPropertyName("albaranes")]
        public bool Albaranes { get; set; }

        [JsonPropertyName("inventarios")]
        public bool Inventarios { get; set; }

        [JsonPropertyName("pedidosRestaurantes")]
        public bool PedidosRestaurantes { get; set; }

        [JsonPropertyName("impresoradeAviso")]
        public string ImpresoradeAviso { get; set; }


        [JsonPropertyName("nombreEmpresa")]
        public string NombreEmpresa { get; set; }

        public bool Seleccionado { get; set; }

        [JsonPropertyName("generico")]
        public bool Generico { get; set; }


        [JsonPropertyName("codPais")]
        public string CodPais { get; set; }

        [JsonPropertyName("centroResp")]
        public string Centro_Resp_ { get; set; }

        [JsonPropertyName("estadisticas")]
        public bool Estadisticas { get; set; }

        [JsonPropertyName("salesCode")]
        public string salesCode { get; set; }

        [JsonPropertyName("eUser")]
        public string EUser { get; set; }

        [JsonPropertyName("ePassword")]
        public string EPassword { get; set; }

        [JsonPropertyName("codigoLocal")]
        public string CodigoLocal { get; set; }

        [JsonPropertyName("codensena")]
        public string CodEnseña { get; set; }

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; }

        [JsonPropertyName("eBalance")]
        public decimal EBalance { get; set; }

        [JsonPropertyName("customerNo")]
        public string CustomerNo { get; set; }

        [JsonPropertyName("vatBusPostingGroup")]
        public string VatBusPostingGroup { get; set; }



        [JsonPropertyName("globalDimension2")]
        public string GlobalDimension2 { get; set; }

        [JsonPropertyName("logo2")]
        public string Logo2 { get; set; }

        [JsonPropertyName("api")]
        public string Api { get; set; }

    }

    public class NavUserLogon
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Usuario")]
        public string Usuario { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("Ultimo_Acceso")]
        public string Ultimo_Acceso { get; set; }

        [JsonPropertyName("Almacen")]
        public string Almacen { get; set; }

        [JsonPropertyName("Token")]
        public string Token { get; set; }




    }

    public class NavUserLogonModif
    {



        [JsonPropertyName("Token")]
        public string Token { get; set; }

        [JsonPropertyName("Dispositivo")]
        public string Dispositivo { get; set; }

        [JsonPropertyName("Fabricante")]
        public string Fabricante { get; set; }

        [JsonPropertyName("NombreDispositivo")]
        public string NombreDispositivo { get; set; }

        [JsonPropertyName("VersionOperativo")]
        public string VersionOperativo { get; set; }

        [JsonPropertyName("Plataforma")]
        public string Plataforma { get; set; }

        [JsonPropertyName("Idioma")]
        public string Idioma { get; set; }

        [JsonPropertyName("TipoDispositivo")]
        public string TipoDispositivo { get; set; }


    }

    public class Aplicacion  
    {

     

        [JsonPropertyName("Application")]
       
        public string Application { get; set; }

        [JsonPropertyName("Multitenancy")]
        public bool Multitenancy { get; set; }

        [JsonPropertyName("Applicaion_URL")]
        public string Applicaion_URL { get; set; }

        [JsonPropertyName("Applicaion_Tenant")]
        public string Applicaion_Tenant { get; set; }

        [JsonPropertyName("Empresa")]
        public string Empresa { get; set; }

        [JsonPropertyName("Nombre_Empresa")]
        public string Nombre_Empresa { get; set; }

        [JsonPropertyName("Usuario")]
        public string Usuario { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("Logo")]
        public string Logo { get; set; }

        [JsonPropertyName("URL_Image_Service")]
        public string URL_Image_Service { get; set; }

        [JsonPropertyName("Odata_Service")]
        public string Odata_Service { get; set; }

        [JsonPropertyName("URL_OAuth")]
        public string URL_OAuth { get; set; }

        [JsonPropertyName("Entorno")]
        public string Entorno { get; set; }

        [JsonPropertyName("App_Type")]
        public string App_Type { get; set; }

        [JsonPropertyName("App_environment")]
        public string App_environment { get; set; }

        [JsonPropertyName("APIGroup")]
        public string APIGroup { get; set; }

        [JsonPropertyName("APIPublisher")]
        public string APIPublisher { get; set; }

        [JsonPropertyName("APIVersion")]
        public string APIVersion { get; set; }

        private bool ocupado;
        private bool ocupadoandroid;

        private bool inicializado;

        public bool Inicializado
        {
            get => inicializado;
            set
            {
                inicializado = value;
               
            }
        }
        public bool Ocupado
        {
            get => ocupado;
            set
            {
                ocupado = value;
                
            }
        }



        public bool OcupadoAndroid
        {
            get => ocupadoandroid;
            set
            {
                ocupadoandroid = value;
                
            }
        }

        string icono;
        public string Icono
        {
            get => icono;
            set
            {
                icono = value;
                
            }

        }

        string userName;
        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
                
            }
        }


    }
    public class AplicacionJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<Aplicacion> Value { get; set; }
    }

    public class NavUserLogonJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<NavUserLogon> Value { get; set; }
    }

    public class Status
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
    }

    public class AppLoginResponse
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<NavUser> Value { get; set; }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class Customer : INotifyPropertyChanged
    {
        private bool ocupado;
        private bool ocupadoandroid;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; }

        [JsonPropertyName("no")]
        public string CustNo { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("postCode")]
        public string PostCode { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("contact")]
        public string contact { get; set; }

        [JsonPropertyName("amount")]
        public decimal amount { get; set; }

        [JsonPropertyName("netChange")]
        public decimal netChange { get; set; }

        [JsonPropertyName("balanceDue")]
        public decimal balanceDue { get; set; }

        [JsonPropertyName("amtShipNotInvLCYBase")]
        public decimal amtShipNotInvLCYBase { get; set; }

        [JsonPropertyName("vatRegistrationNo")]
        public string vatRegistrationNo { get; set; }

        [JsonPropertyName("phoneNo")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("shiptoCode")]
        public string Ship_to_Code { get; set; }

        [JsonPropertyName("eMail")]
        public string EMail { get; set; }

        [JsonPropertyName("creditLimitLCY")]
        public decimal creditLimitLCY { get; set; }
        public bool Ocupado
        {
            get => ocupado;
            set
            {
                ocupado = value;
                NotifyPropertyChanged("Ocupado");
            }
        }

        public bool OcupadoAndroid
        {
            get => ocupadoandroid;
            set
            {
                ocupadoandroid = value;
                NotifyPropertyChanged("OcupadoAndroid");
            }
        }



        public double SumAmount { get; set; }




        [JsonPropertyName("discountGeneral")]
        public double Desgeneral { get; set; }




        [JsonPropertyName("discountPP")]
        public double DescPP { get; set; }


    }
    public class CustomerJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<Customer> Value { get; set; }
    }

    public class CustomerAddres : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool chequeado;
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }


        public string AddressType { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }


        public string MobileNumber { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("customerNo")]
        public string CustomerNo { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("name2")]
        public string Name2 { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("postCode")]
        public string PostCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("contact")]
        public string Contact { get; set; }

        public bool Chequeado
        {
            get => chequeado;
            set
            {
                chequeado = value;
                NotifyPropertyChanged("Chequeado");
            }
        }


    }
    public class CustomerAddresJSON
    {
        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<CustomerAddres> Value { get; set; }
    }

    public class FamiliasCloudJson
    {
        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<FamliasCloud> Value { get; set; }
    }
    public class FamliasCloud : INotifyPropertyChanged
    {
        private bool ocupado;
        private bool ocupadoandroid;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
      

        [JsonPropertyName("Familia")]
        public string Famlia { get; set; }

        [JsonPropertyName("Descripcion")]
        public string Descripcion { get; set; }

   
        private int order;
        [JsonPropertyName("Order")]
        public int Order
        {
            get => order;
            set
            {
                order = value;
                NotifyPropertyChanged("Order");
            }
        }

        public bool Ocupado
        {
            get => ocupado;
            set
            {
                ocupado = value;
                NotifyPropertyChanged("Ocupado");
            }
        }

        [JsonProperty("subfamlines")]
        public IList<SubFamilia> subfamlines { get; set; }


    }
    public class SubFamilia : INotifyPropertyChanged
    {
        private bool ocupado;
        private bool ocupadoandroid;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

        [JsonPropertyName("No")]
        public string No { get; set; }

 
        [JsonPropertyName("Description")]
        public string Descripcion { get; set; }

        
        public bool Ocupado
        {
            get => ocupado;
            set
            {
                ocupado = value;
                NotifyPropertyChanged("Ocupado");
            }
        }

         
    }

 

    public class ProductCloud : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region Fields


        private bool ocupado;
        private bool ocupadoandroid;






        private int totalQuantity;




        private double tipoIva;

        #endregion

        #region Properties



        
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("itemNo")]
        public string No { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("webDescription")]
        public string Summary { get; set; }


        [JsonPropertyName("vendorNo")]
        public string VendorNo { get; set; }


        [JsonPropertyName("unitofMeasureCode")]
        public string UnitofMeasureCode { get; set; }


        [JsonPropertyName("Familia")]
        public string Familia { get; set; }


        [JsonPropertyName("FamiliaN")]
        public string FamiliaN { get; set; }


        [JsonPropertyName("SubFamilia")]
        public string SubFamilia { get; set; }


        [JsonPropertyName("directUnitCost")]
        public double directUnitCost { get; set; }


        [JsonPropertyName("locationCode")]
        public string LocationCode { get; set; }


        [JsonPropertyName("tipo")]
        public string OrderType { get; set; }


        [JsonPropertyName("inventario100")]
        public double inventario100 { get; set; }


        [JsonPropertyName("inventarioNo100")]
        public double inventarioNo100 { get; set; }



        [JsonPropertyName("descriptionprod")]
        public string Description { get; set; }

        [JsonPropertyName("ingredientes")]
        public string ingredientes { get; set; }

        public double actualprice;

        [JsonPropertyName("unitPrice")]
        public double ActualPrice
        {
            get => actualprice;
            set
            {
                actualprice = value;
                NotifyPropertyChanged("ActualPrice");
            }
        }



        [JsonPropertyName("tipoIva")]
        public double TipoIva
        {
            get => tipoIva;
            set
            {
                tipoIva = value;
                NotifyPropertyChanged("TipoIva");
            }
        }

        public double DiscountPercent { get; set; }


        public double OverallRating { get; set; }

      
        public int TotalQuantity
        {
            get => totalQuantity;
            set
            {
                totalQuantity = value;
                NotifyPropertyChanged("TotalQuantity");
            }
        }

        private double cantidadencarro;
     
        public double Cantidadencarro
        {
            get => cantidadencarro;
            set
            {
                cantidadencarro = value;
                NotifyPropertyChanged("Cantidadencarro");
            }
        }



        [JsonPropertyName("tipoPrep")]
        public int TipoPrep { get; set; }

        [JsonPropertyName("EAN13")]
        public string EAN13 { get; set; }

        [JsonPropertyName("ImageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("recomendado")]
        public bool Recomendado { get; set; }
        public string Category { get; set; }
        public double DiscountPrice
        {
            get => ActualPrice - ActualPrice * (DiscountPercent / 100);
            set
            {

            }
        }
        public double Importe
        {
            get => ActualPrice * Cantidad;


        }
        [JsonPropertyName("imagen")]
        public string imagen { get; set; }



        [JsonPropertyName("descFamily")]
        public string Desc_Family { get; set; }


        public bool Ocupado
        {
            get => ocupado;
            set
            {
                ocupado = value;
                NotifyPropertyChanged("Ocupado");
            }
        }



        public bool OcupadoAndroid
        {
            get => ocupadoandroid;
            set
            {
                ocupadoandroid = value;
                NotifyPropertyChanged("OcupadoAndroid");
            }
        }


        [JsonPropertyName("inventoryPostingGroup")]
        public string InventoryPostingGroup { get; set; }

        [JsonPropertyName("stockUnitofMeasure")]
        public string StockUnitofMeasure { get; set; }

        [JsonPropertyName("netChange")]
        public double NetChange { get; set; }


        [JsonPropertyName("unitCost")]
        public double UnitCost { get; set; }


        [JsonPropertyName("Presentation_Qty")]
        public double Presentation_Qty { get; set; }

        [JsonPropertyName("cantidadpor")]
        public double Cantidadpor { get; set; }

        public double Unidades
        {

            get => NetChange / Cantidadpor;
        }

        private double precio;
        public double Precio
        {
            get => UnitCost * Cantidadpor;
            set
            {
                precio = value;
                ActualPrice = precio;
                NotifyPropertyChanged("Precio");
            }
        }

        public int Cantidad { get; set; }


        [JsonPropertyName("parentWebFamily")]
        public string FamilyCode { get; set; }

        [JsonPropertyName("periodicidad")]
        public string Periodicidad { get; set; }

        [JsonPropertyName("tipoProducto")]
        public string TipoProducto { get; set; }

        private string color;

        public string Color
        {
            get => color;
            set
            {
                color = value;
                NotifyPropertyChanged("Color");
            }
        }

        private double? cantidadcontada;
        public double? CantidadContada
        {
            get => cantidadcontada;
            set
            {
                cantidadcontada = value;
                NotifyPropertyChanged("CantidadContada");
            }
        }

        private double? cantidadfisica;
        public double? CantidadFisica
        {
            get => cantidadfisica;
            set
            {
                cantidadfisica = value;
                CantidadContada += cantidadfisica;
                NotifyPropertyChanged("CantidadFisica");
            }
        }



        private string mediaReadLink;

        [JsonPropertyName("Imagen_App@odata.mediaReadLink")]
        public string MediaReadLink
        {
            get => mediaReadLink;
            set
            {
                mediaReadLink = value;
                NotifyPropertyChanged("MediaReadLink");


            }
        }

        private bool tieneimg;
        public bool tieneimagen
            {
            get => tieneimg;
            set
            {
                tieneimg = value;
                NotifyPropertyChanged("tieneimagen");
            }
        }





        private bool promotion;

        [JsonPropertyName("Promotion")]
        public bool Promotion
        {
            get => promotion;
            set
            {
                promotion = value;
                NotifyPropertyChanged("Promotion");


            }
        }




        private string present_Unit;

        [JsonPropertyName("Present_Unit")]
        public string Present_Unit
        {
            get => present_Unit;
            set
            {
                present_Unit = value;
                NotifyPropertyChanged("Present_Unit");


            }
        }




        private double presentation_Price;
        [JsonPropertyName("Presentation_Price")]
        public double Presentation_Price
        {
            get => presentation_Price;
            set
            {
                presentation_Price = value;
                NotifyPropertyChanged("Presentation_Price");


            }
        }




        private double descuento;
        [JsonPropertyName("Descuento")]
        public double Descuento
        {
            get => descuento;
            set
            {
                descuento = value;
                NotifyPropertyChanged("Descuento");


            }
        }
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


       
        #endregion
    }
    public class ImagenProducto : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool actualizado;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
        public int id { get; set; }

        [JsonPropertyName("itemNo")]
        public string No { get; set; }


        public byte[] imagenb { get; set; }

        public bool Actualizado
        {
            get => actualizado;
            set
            {
                actualizado = value;
                NotifyPropertyChanged("Actualizado");
            }
        }

        private string mediaReadLink;

        [JsonPropertyName("Imagen_App@odata.mediaReadLink")]
        public string MediaReadLink
        {
            get => mediaReadLink;
            set
            {
                mediaReadLink = value;

            }
        }

    }


    public class ProductoJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<Product> Value { get; set; }
    }

    public class ProductoCloudJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<ProductCloud> Value { get; set; }
    }

    public class ImagenJson
    {

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("value")]
        public IList<ImagenProducto> Value { get; set; }
    }
    public class EstadisticasJson
    {

        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public IList<Estadisticas> Value { get; set; }
    }
    public class Estadisticas
    {
        public string Vendedor { get; set; }
        public string Cliente { get; set; }
        public string Nombre { get; set; }
        public double Ventas { get; set; }
        public double Compras { get; set; }
        public double Ratio { get; set; }
        public double VentasAA { get; set; }
        public double VentasAAA { get; set; }
        public double VentasPresup { get; set; }
        public int orden { get; set; }
        public string Tipo { get; set; }

    }
}
