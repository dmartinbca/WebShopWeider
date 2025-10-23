using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class ClientSelectionService
{
    private readonly LocalStorageService _store;
    private readonly RestDataService _rest;
    private const string KEY_SELECTED_CLIENT = "salesteam.selectedclient";
    
    public Customer? SelectedClient { get; private set; }
    public event Action? OnClientChanged;
    
    public ClientSelectionService(LocalStorageService store, RestDataService rest)
    {
        _store = store;
        _rest = rest;
    }
    
    public async Task InitializeAsync()
    {
        var saved = await _store.GetAsync<Customer>(KEY_SELECTED_CLIENT);
        SelectedClient = saved;
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
        }
        else
        {
            await _store.SetAsync(KEY_SELECTED_CLIENT, client);
        }
        
        OnClientChanged?.Invoke();
    }
    
    public async Task ClearSelection()
    {
        await SelectClient(null);
    }
}
