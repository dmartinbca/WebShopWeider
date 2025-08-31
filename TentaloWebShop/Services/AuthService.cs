using System.Net.NetworkInformation;
using TentaloWebShop.Models;

namespace TentaloWebShop.Services;

public class AuthService
{
    private readonly LocalStorageService _store;

    private const string KEY_USER = "auth.currentUser";
    private const string KEY_USERS = "auth.users";

    private const string KEY_CUSTOMER = "auth.currentCustomer";
    private const string KEY_CUSTOMERS = "auth.customer";



    private bool _initialized;
    public bool IsAuthenticated => CurrentUser is not null;
    public User? CurrentUser { get; private set; }

    public Customer? CurrentCustomer { get; private set; }

    private List<(User user, string password)> _users = new();

    private List<(Customer cust,string cod)> _u_custsers = new();
    private readonly RestDataService _rest;
   
    public AuthService(LocalStorageService store, RestDataService rest)
    {
               _store = store;
        _rest = rest;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            var raw = await _store.GetAsync<List<User>>(KEY_USERS) ?? new();
            var raw1 = await _store.GetAsync<List<Customer>>(KEY_CUSTOMERS) ?? new();

            var saved = await _store.GetAsync<User>(KEY_USER);
            var saved1 = await _store.GetAsync<Customer>(KEY_CUSTOMER);
            _users = raw.Select(u => (u, "demo")).ToList(); // demo password
            CurrentUser = saved;
            CurrentCustomer = saved1;
        }
        catch { /* no re-lanzar en startup */ }
        _initialized = true;
    }

    private Task PersistUsers()
        => _store.SetAsync(KEY_USERS, _users.Select(u => u.user).ToList());

    private Task PersistCustomers()
       => _store.SetAsync(KEY_CUSTOMERS, _u_custsers.Select(u => u.cod).ToList());

    private Task PersistCurrent(User? u)
        => u is null ? _store.RemoveAsync(KEY_USER) : _store.SetAsync(KEY_USER, u);

    private Task PersistCurrentCust(Customer? u)
      => u is null ? _store.RemoveAsync(KEY_CUSTOMER) : _store.SetAsync(KEY_CUSTOMER, u);

    public async Task<bool> Register(User user, string password)
    {
        await InitializeAsync();
        if (_users.Any(u => u.user.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) return false;
        _users.Add((user, password));
        await PersistUsers();
        return true;
    }

    public async Task<bool> Login(string email, string password)
    {
        await InitializeAsync();
        NavUser response = new NavUser();
         
        var euser= await _rest.GetAppLoginAPICloud(email,password);

       // var match = _users.FirstOrDefault(u => u.user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (euser is not null )
        {
            var eAddress = await _rest.GetDirecciones(euser.CustomerNo);

            var parts = email.Split('@');
            if (eAddress==null)
            {
                eAddress = new List<CustomerAddres>();
            }
               
            var newUser = new User { Email = euser.Usuario,   FullName=euser.CustomerName, CustomerNo=euser.CustomerNo, EsMaster=euser.EsMaster, CustomerAddres=eAddress};
            _users.Add((newUser, password));
            await PersistUsers();

            CurrentUser = newUser;
            await PersistCurrent(CurrentUser);

            var cust = await _rest.GetCustomersAPI(euser.CustomerNo);
            _u_custsers.Add((cust.FirstOrDefault(),cust.FirstOrDefault().CustNo ));
            await PersistCustomers();
            CurrentCustomer= cust.FirstOrDefault();
            await PersistCurrentCust(CurrentCustomer);
            return true;
        }
         
        return false;
    }

    public async Task Logout()
    {
        await InitializeAsync();
        CurrentUser = null;
        await PersistCurrent(null);
    }
}
