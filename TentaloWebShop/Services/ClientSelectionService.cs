using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ClientSelectionService
{
    private readonly LocalStorageService _store;
    private readonly RestDataService _rest;
    private readonly AuthService _auth;
    private const string KEY_SELECTED_CLIENT = "salesteam.selectedclient";

    public Customer? SelectedClient { get; private set; }
    public event Action? OnClientChanged;

    public ClientSelectionService(LocalStorageService store, RestDataService rest, AuthService auth)
    {
        _store = store;
        _rest = rest;
        _auth = auth;
    }

    public async Task InitializeAsync()
    {
        var saved = await _store.GetAsync<Customer>(KEY_SELECTED_CLIENT);
        SelectedClient = saved;

        // Si hay un cliente guardado y el usuario es Sales Team, actualizar AuthService
        if (saved != null && _auth.CurrentUser?.Tipo == "Sales Team")
        {
            await _auth.SetCurrentCustomer(saved);
        }
    }

    public async Task<List<Customer>> GetClientsForSalesperson(string salesCode)
    {
        try
        {
            string fecha = DateTime.Now.ToString("yyyy-MM-dd");
            var clients = await _rest.GetCustomersAPI(salesCode, fecha);
            return clients;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR GetClientsForSalesperson] {ex.Message}");
            return new List<Customer>();
        }
    }

    public async Task SelectClient(Customer? client)
    {
        SelectedClient = client;

        if (client is null)
        {
            await _store.RemoveAsync(KEY_SELECTED_CLIENT);
            // Si es Sales Team, mantener su propio customer
            if (_auth.CurrentUser?.Tipo == "Sales Team")
            {
                // Cargar el customer del vendedor
                var vendedorCustomer = await _rest.GetCustomersAPI(_auth.CurrentUser.CustomerNo);
                await _auth.SetCurrentCustomer(vendedorCustomer.FirstOrDefault());
            }
        }
        else
        {
            await _store.SetAsync(KEY_SELECTED_CLIENT, client);
            // Actualizar el CurrentCustomer en AuthService
            await _auth.SetCurrentCustomer(client);
        }

        OnClientChanged?.Invoke();
    }

    public async Task ClearSelection()
    {
        await SelectClient(null);
    }
}
