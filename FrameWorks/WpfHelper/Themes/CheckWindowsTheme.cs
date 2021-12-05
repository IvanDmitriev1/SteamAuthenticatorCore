using Microsoft.Win32;

namespace WpfHelper.Themes
{
    public enum WindowsThemes
    {
        Dark,
        Light
    }

    public static class CheckWindowsTheme
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";


        /// <summary>
        /// Gets windows current theme
        /// </summary>
        /// <returns></returns>
        public static WindowsThemes GetWindowsTheme()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            object? registryValueObject = key?.GetValue(RegistryValueName);

            if (registryValueObject == null)
                return WindowsThemes.Light;

            int registryValue = (int)registryValueObject;

            return registryValue > 0 ? WindowsThemes.Light : WindowsThemes.Dark;
        }
    }
}
