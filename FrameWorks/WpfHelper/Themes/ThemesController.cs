using System;
using System.Windows;

namespace WpfHelper.Themes
{
    public class ThemeLocation
    {
        public string ThemeFilesLocation { get; set; }

        public int ThemePosition { get; set; }

        public ThemeLocation()
        {
            ThemeFilesLocation = "Themes";
            ThemePosition = 0;
        }
    }

    public enum WpfHelperThemes
    {
        DarkTheme,
        ColourfulDarkTheme,
        LightTheme,
        ColourfulLightTheme
    }

    public class ThemesController
    {
        /// <summary>
        /// Default ThemeFilesLocation is Themes, Default ThemePosition is 0
        /// </summary>
        public static ThemeLocation ThemeLocation { get; set; } = new();

        public static string CurrentTheme { get; private set; } = GetCurrentTheme();

        public static bool UseWpfHelperThemes
        {
            set => ThemeLocation.ThemeFilesLocation = "pack://application:,,,/WpfHelper;component/Themes";
        }

        private static ResourceDictionary ThemeDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[ThemeLocation.ThemePosition];
            set => Application.Current.Resources.MergedDictionaries[ThemeLocation.ThemePosition] = value;
        }

        /// <summary>
        /// Uses CheckWindowsTheme to set WpfHelper themes and sets UseWpfHelperThemes = true
        /// </summary>
        public static void Init()
        {
			UseWpfHelperThemes = true;
			
            switch (CheckWindowsTheme.GetWindowsTheme())
            {
                case WindowsThemes.Dark:
                    SetTheme(WpfHelperThemes.ColourfulDarkTheme);
                    break;
                case WindowsThemes.Light:
                    SetTheme(WpfHelperThemes.ColourfulLightTheme);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets your themes
        /// </summary>
        /// <param name="themeName"></param>
        /// <returns>A status of success</returns>
        public static bool SetTheme(string themeName)
        {
            if (themeName == CurrentTheme) return false;

            CurrentTheme = themeName;
            SettingNewTheme(themeName);
            return true;
        }

        /// <summary>
        /// Sets WpfHelper themes
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static bool SetTheme(WpfHelperThemes theme)
        {
            if (theme.ToString().Length == CurrentTheme.Length) return false;

            CurrentTheme = theme.ToString();
            SettingNewTheme(theme.ToString());
            return true;
        }

        private static string GetCurrentTheme()
        {
            string s = Application.Current.Resources.MergedDictionaries[ThemeLocation.ThemePosition].Source.OriginalString;

            s = s.Substring(s.LastIndexOf('/') + 1);
            s = s.Remove(s.IndexOf('.'));

            return s;
        }

        private static void SettingNewTheme(string themeName)
        {
            try
            {
                ThemeDictionary = new ResourceDictionary()
                {
                    Source = new Uri($"{ThemeLocation.ThemeFilesLocation}/{themeName}.xaml", UriKind.RelativeOrAbsolute)
                };
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception in {nameof(ThemesController)} - {e.Message}");
                throw;
            }
        }
    }
}
