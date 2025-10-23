using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class AuthService
{
    private readonly LocalStorageService _store;
    private readonly RestDataService _rest;
    private const string KEY_USER = "auth.currentUser";
    private const string KEY_CUSTOMER = "auth.currentCustomer";

    public User? CurrentUser { get; private set; }
    public Customer? CurrentCustomer { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    // IMPORTANTE: Evento que notifica cuando cambia el cliente
    public event Action? OnCustomerChanged;
    public event Action? OnAuthStateChanged;

    public AuthService(LocalStorageService store, RestDataService rest)
    {
        _store = store;
        _rest = rest;
    }

    public async Task InitializeAsync()
    {
        try
        {
            CurrentUser = await _store.GetAsync<User>(KEY_USER);
            CurrentCustomer = await _store.GetAsync<Customer>(KEY_CUSTOMER);

            Console.WriteLine($"[AuthService.InitializeAsync] User: {CurrentUser?.Email}, Customer: {CurrentCustomer?.Name}");

            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR AuthService.InitializeAsync] {ex.Message}");
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var navUser = await _rest.GetAppLoginAPICloud(email, password);
            if (navUser == null) return false;

            // Crear el objeto User
            CurrentUser = new User
            {
                Email = navUser.EMail,
                FullName = navUser.Name ?? navUser.Usuario,
                FirstName = navUser.Name?.Split(' ').FirstOrDefault() ?? "",
                LastName = navUser.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Token = navUser.Token ?? "",
                Tipo = navUser.Tipo,
                EsMaster = navUser.EsMaster,
                salesCode = navUser.salesCode ?? "",
                CustomerNo = navUser.CustomerNo ?? "",
                CustomerName = navUser.CustomerName ?? "",
                VatBusPostingGroup = navUser.VatBusPostingGroup ?? "",
                DescuentoFactura = navUser.Descuento_en_factura,
                DescuentoPP = 0
            };

            // Guardar usuario
            await _store.SetAsync(KEY_USER, CurrentUser);

            // Cargar el cliente asociado SOLO si NO es Sales Team
            // Los Sales Team seleccionar�n su cliente manualmente
            if (CurrentUser.Tipo != "Sales Team" && !string.IsNullOrEmpty(CurrentUser.CustomerNo))
            {
                var clientes = await _rest.GetCustomersAPI(CurrentUser.CustomerNo);
                if (clientes?.Count > 0)
                {
                    await SetCurrentCustomer(clientes[0]);
                }
            }

            OnAuthStateChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR AuthService.LoginAsync] {ex.Message}");
            return false;
        }
    }

    public async Task SetCurrentCustomer(Customer? customer)
    {
        var previousCustomer = CurrentCustomer?.CustNo;
        CurrentCustomer = customer;

        if (customer != null)
        {
            await _store.SetAsync(KEY_CUSTOMER, customer);
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Cliente cambiado a: {customer.Name} ({customer.CustNo})");
        }
        else
        {
            await _store.RemoveAsync(KEY_CUSTOMER);
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Cliente eliminado");
        }

        // CR�TICO: Solo disparar el evento si el cliente realmente cambi�
        if (previousCustomer != customer?.CustNo)
        {
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Disparando OnCustomerChanged");
            OnCustomerChanged?.Invoke();
            OnAuthStateChanged?.Invoke();
        }
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        CurrentCustomer = null;
        await _store.RemoveAsync(KEY_USER);
        await _store.RemoveAsync(KEY_CUSTOMER);
        OnAuthStateChanged?.Invoke();
    }
}
