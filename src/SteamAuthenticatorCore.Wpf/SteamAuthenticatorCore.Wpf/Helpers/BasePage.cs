using Microsoft.Extensions.DependencyInjection;

namespace SteamAuthenticatorCore.Desktop.Helpers;

public abstract class BasePage<T> : Page where T : ObservableRecipient
{
    protected BasePage()
    {
        _asyncServiceScope = App.ServiceProvider.CreateAsyncScope();

        ViewModel = _asyncServiceScope.ServiceProvider.GetRequiredService<T>();
        DataContext = ViewModel;

        Loaded += static (sender, _) =>
        {
            var self = (BasePage<T>)sender;
            self.OnLoaded();
        };

        Unloaded += static (sender, _) =>
        {
            var self = (BasePage<T>)sender;
            self.OnUnloaded();
        };
    }

    private readonly AsyncServiceScope _asyncServiceScope;

    public T ViewModel { get; }

    public virtual void OnLoaded()
    {
        ViewModel.IsActive = true;

        Debug.WriteLine($"Loaded: {GetType()}");
    }

    public virtual async void OnUnloaded()
    {
        ViewModel.IsActive = false;

        await _asyncServiceScope.DisposeAsync();

        Debug.WriteLine($"UnLoaded: {GetType()}");
    }
}