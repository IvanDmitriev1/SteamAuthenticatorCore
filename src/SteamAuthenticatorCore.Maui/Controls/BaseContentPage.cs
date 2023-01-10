using System.Diagnostics;

namespace SteamAuthenticatorCore.Mobile.Controls;

public abstract class BaseContentPage<TViewModel> : BaseContentPage
{
    protected BaseContentPage(TViewModel viewModel) : base(viewModel)
    {
    }

    public new TViewModel BindingContext => (TViewModel)base.BindingContext;
}

public abstract class BaseContentPage : ContentPage
{
    protected BaseContentPage(object? viewModel = null)
    {
        BindingContext = viewModel;

        Loaded += (_, _) =>
        {
            if (Shell.GetTitleView(this) is null)
                Shell.SetTitleView(this, new MyTitleView(Title));

            if (string.IsNullOrEmpty(Title))
                Title = GetType().Name;
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Debug.WriteLine($"OnAppearing: {Title}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        Debug.WriteLine($"OnDisappearing: {Title}");
    }
}