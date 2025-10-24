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

    public event Func<Task>? OnCustomerChanged;
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
                DescuentoPP = 0,
                IdiomaPais = navUser.CodPais,
                CustomerAddres = new List<CustomerAddres>()
            };

            // Guardar usuario
            await _store.SetAsync(KEY_USER, CurrentUser);

            // Cargar el cliente asociado SOLO si NO es Sales Team
            if (CurrentUser.Tipo != "Sales Team" && !string.IsNullOrEmpty(CurrentUser.CustomerNo))
            {
                var clientes = await _rest.GetCustomersAPI(CurrentUser.CustomerNo);
                if (clientes?.Count > 0)
                {
                    // ✅ SetCurrentCustomer cargará automáticamente las direcciones
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
        var previousCustNo = CurrentCustomer?.CustNo;
        var newCustNo = customer?.CustNo;

        bool hasChanged = previousCustNo != newCustNo;

        CurrentCustomer = customer;

        if (customer != null)
        {
            await _store.SetAsync(KEY_CUSTOMER, customer);
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Cliente establecido: {customer.Name} ({customer.CustNo})");

            // ✅ AQUÍ CARGAMOS LAS DIRECCIONES DEL CLIENTE
            if (CurrentUser != null)
            {
                try
                {
                    Console.WriteLine($"[AuthService.SetCurrentCustomer] Cargando direcciones para cliente: {customer.CustNo}");
                    var direcciones = await _rest.GetDirecciones(customer.CustNo);

                    if (direcciones?.Any() == true)
                    {
                        CurrentUser.CustomerAddres = direcciones;
                        Console.WriteLine($"[AuthService.SetCurrentCustomer] ✅ Direcciones cargadas: {direcciones.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"[AuthService.SetCurrentCustomer] ⚠️ No hay direcciones para este cliente");
                        CurrentUser.CustomerAddres = new List<CustomerAddres>();
                    }

                    // Guardar el usuario actualizado con las direcciones
                    await _store.SetAsync(KEY_USER, CurrentUser);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR AuthService.SetCurrentCustomer] Cargando direcciones: {ex.Message}");
                    CurrentUser.CustomerAddres = new List<CustomerAddres>();
                }
            }
        }
        else
        {
            await _store.RemoveAsync(KEY_CUSTOMER);
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Cliente eliminado");

            // Limpiar direcciones si se elimina el cliente
            if (CurrentUser != null)
            {
                CurrentUser.CustomerAddres = new List<CustomerAddres>();
                await _store.SetAsync(KEY_USER, CurrentUser);
            }
        }

        // ✅ Disparar el evento SI cambió
        if (hasChanged)
        {
            Console.WriteLine($"[AuthService.SetCurrentCustomer] Cliente cambió de {previousCustNo} a {newCustNo}. Disparando OnCustomerChanged");

            if (OnCustomerChanged != null)
            {
                try
                {
                    var tasks = OnCustomerChanged.GetInvocationList()
                        .Cast<Func<Task>>()
                        .Select(f => f.Invoke());

                    await Task.WhenAll(tasks);
                    Console.WriteLine($"[AuthService.SetCurrentCustomer] OnCustomerChanged completado");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR AuthService.SetCurrentCustomer] Error en OnCustomerChanged: {ex.Message}");
                }
            }

            OnAuthStateChanged?.Invoke();
        }
        else
        {
            Console.WriteLine($"[AuthService.SetCurrentCustomer] El cliente no cambió. No se dispara el evento.");
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