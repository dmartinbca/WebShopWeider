namespace TentaloWebShop.Services;

public class LoadingService
{
    private bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnLoadingChanged?.Invoke();
            }
        }
    }

    public event Action? OnLoadingChanged;

    public async Task ExecuteWithLoadingAsync(Func<Task> operation)
    {
        try
        {
            IsLoading = true;
            await operation();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            IsLoading = true;
            return await operation();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
