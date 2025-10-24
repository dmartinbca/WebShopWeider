using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ShippingAddressService
{
    private readonly RestDataService _rest;
    private readonly AuthService _auth;
    private List<ShippingAddress>? _cache;
    private string? _lastCustomerNo;

    public List<ShippingAddress> Addresses { get; private set; } = new();
    public ShippingAddress? SelectedAddress { get; private set; }
    
    public event Action? OnAddressesLoaded;
    public event Action? OnAddressSelected;

    public ShippingAddressService(RestDataService rest, AuthService auth)
    {
        _rest = rest;
        _auth = auth;
        
        // Suscribirse a cambios de cliente para limpiar caché
        _auth.OnCustomerChanged += OnCustomerChanged;
    }

    private async Task OnCustomerChanged()
    {
        Console.WriteLine("[ShippingAddressService.OnCustomerChanged] Cliente cambió, limpiando direcciones");
        ClearCache();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Carga las direcciones de envío para el cliente actual
    /// </summary>
    public async Task<List<ShippingAddress>> LoadAddressesAsync(string? customerNo = null)
    {
        try
        {
            // Usar el customerNo proporcionado o el actual
            string efectiveCustomerNo = customerNo ?? GetEffectiveCustomerNo();

            if (string.IsNullOrWhiteSpace(efectiveCustomerNo))
            {
                Console.WriteLine("[ShippingAddressService] No hay cliente para cargar direcciones");
                Addresses = new List<ShippingAddress>();
                return Addresses;
            }

            // Si cambió el cliente, limpiar caché
            if (_lastCustomerNo != efectiveCustomerNo)
            {
                _cache = null;
                _lastCustomerNo = efectiveCustomerNo;
            }

            // Usar caché si está disponible
            if (_cache != null)
            {
                Console.WriteLine($"[ShippingAddressService] Usando caché - {_cache.Count} direcciones");
                Addresses = _cache;
                return Addresses;
            }

            Console.WriteLine($"[ShippingAddressService] Cargando direcciones para cliente: {efectiveCustomerNo}");

            // ✅ USAR GetDirecciones del RestDataService
            var direcciones = await _rest.GetDirecciones(efectiveCustomerNo);
            
            if (direcciones != null && direcciones.Count > 0)
            {
                // Mapear desde el tipo que retorna GetDirecciones a ShippingAddress
                Addresses = MapDireccionesToShippingAddresses(direcciones);
                _cache = Addresses;

                Console.WriteLine($"[ShippingAddressService] Cargadas {Addresses.Count} direcciones");
            }
            else
            {
                Addresses = new List<ShippingAddress>();
                Console.WriteLine($"[ShippingAddressService] No se encontraron direcciones para {efectiveCustomerNo}");
            }

            OnAddressesLoaded?.Invoke();
            return Addresses;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR ShippingAddressService.LoadAddressesAsync] {ex.Message}");
            Addresses = new List<ShippingAddress>();
            return Addresses;
        }
    }

    /// <summary>
    /// Mapea las direcciones retornadas por GetDirecciones a ShippingAddress
    /// ⚠️ AJUSTA ESTO A TU MODELO DE DATOS REAL
    /// </summary>
    private List<ShippingAddress> MapDireccionesToShippingAddresses(List<CustomerAddres> direcciones)
    {
        var result = new List<ShippingAddress>();

        try
        {
            foreach (var dir in direcciones)
            {
                result.Add(new ShippingAddress
                {
                    Code = dir.Code?.ToString() ?? "",
                    Description = dir.Name?.ToString() ?? dir.Name?.ToString() ?? "",
                    Address = dir.Address?.ToString() ?? "",
                    City = dir.City?.ToString() ?? "",
                    PostCode = dir.PostCode?.ToString() ?? dir.PostCode?.ToString() ?? "",
                    County = dir.County?.ToString() ?? dir.County?.ToString() ?? "",
                    Country = dir.County?.ToString() ?? "",
                    Phone = dir.MobileNumber?.ToString() ?? "",
                    ContactPerson = dir.Contact?.ToString() ?? dir.Contact?.ToString() ?? "" 
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR ShippingAddressService.MapDireccionesToShippingAddresses] {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Selecciona una dirección de envío
    /// </summary>
    public void SelectAddress(ShippingAddress? address)
    {
        SelectedAddress = address;
        Console.WriteLine($"[ShippingAddressService] Dirección seleccionada: {address?.Description}");
        OnAddressSelected?.Invoke();
    }

    /// <summary>
    /// Obtiene la dirección seleccionada o la primera como predeterminada
    /// </summary>
    public ShippingAddress? GetEffectiveAddress()
    {
        return SelectedAddress ?? Addresses.FirstOrDefault();
    }

    /// <summary>
    /// Limpia el caché
    /// </summary>
    public void ClearCache()
    {
        _cache = null;
        _lastCustomerNo = null;
        Addresses = new List<ShippingAddress>();
        SelectedAddress = null;
    }

    /// <summary>
    /// Determina el código de cliente efectivo
    /// </summary>
    private string GetEffectiveCustomerNo()
    {
        return _auth?.CurrentCustomer?.CustNo ?? _auth?.CurrentUser?.CustomerNo ?? "";
    }
}
