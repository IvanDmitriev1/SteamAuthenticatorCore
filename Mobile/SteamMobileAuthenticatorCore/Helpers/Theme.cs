using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Helpers
{
    public static class Theme
    {
        public static int TheTheme
        {
            get => Preferences.Get(nameof(Theme), 0);
            set => Preferences.Set(nameof(Theme), value);
        }

        public static void SetTheme()
        {
            switch (TheTheme)
            {
                case 0:
                    Application.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;
                case 1:
                    App.Current.UserAppTheme = OSAppTheme.Light;
                    break;
                case 2:
                    App.Current.UserAppTheme = OSAppTheme.Dark;
                    break;
            }

            var environment = DependencyService.Get<IEnvironment>();

            if (Application.Current.RequestedTheme == OSAppTheme.Dark)
                environment.SetStatusBarColor(Color.Black, false);
            else
                environment.SetStatusBarColor(Color.White, true);
        }
    }
}
