namespace WeiderShop.Services;

public class BusyService
{
    public event Action? Changed;
    public bool IsBusy { get; private set; }
    public string Message { get; private set; } = "Cargando…";

    public void Show(string? message = null)
    {
        IsBusy = true;
        if (!string.IsNullOrWhiteSpace(message)) Message = message!;
        Changed?.Invoke();
    }
    public void Hide()
    {
        IsBusy = false;
        Changed?.Invoke();
    }
}
