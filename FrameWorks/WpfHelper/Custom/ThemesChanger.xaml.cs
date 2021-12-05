using System.Windows;
using System.Windows.Controls;
using WpfHelper.Themes;

namespace WpfHelper.Custom
{
    /// <summary>
    /// For usage set ThemesController.UseWpfHelperThemes = true
    /// </summary>
    public partial class ThemesChanger : UserControl
    {
        public ThemesChanger()
        {
            InitializeComponent();
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;

            switch (menuItem.Uid)
            {
                case "0":
                    ThemesController.SetTheme(WpfHelperThemes.LightTheme);
                    break;
                case "1":
                    ThemesController.SetTheme(WpfHelperThemes.ColourfulLightTheme);
                    break;
                case "2":
                    ThemesController.SetTheme(WpfHelperThemes.DarkTheme);
                    break;
                case "3":
                    ThemesController.SetTheme(WpfHelperThemes.ColourfulDarkTheme);
                    break;
            }

            e.Handled = true;
        }
    }
}
