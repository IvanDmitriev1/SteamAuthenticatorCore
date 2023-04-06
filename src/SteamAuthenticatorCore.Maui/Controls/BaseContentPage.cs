namespace SteamAuthenticatorCore.Maui.Controls;

public abstract class BaseContentPage<TViewModel> : ContentPage where TViewModel : ObservableRecipient
{
    protected BaseContentPage(TViewModel viewModel)
    {
        base.BindingContext = viewModel;

        Loaded += static (sender, _) =>
        {
            var self = (BaseContentPage<TViewModel>)sender!;
            self.OnLoaded();
        };

#if DEBUG
        if (string.IsNullOrWhiteSpace(Title))
            Title = GetType().Name;
#endif
    }

    public new TViewModel BindingContext => (TViewModel)base.BindingContext;

    protected virtual void OnLoaded()
    {
        if (Shell.GetTitleView(this) is not MyTitleView titleView)
        {
            Shell.SetTitleView(this, new MyTitleView(Title));
        }
        else
        {
            titleView.TitleName = Title;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        BindingContext.IsActive = true;

        Debug.WriteLine($"OnAppearing: {Title}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        BindingContext.IsActive = false;

        Debug.WriteLine($"OnDisappearing: {Title}");
    }
}