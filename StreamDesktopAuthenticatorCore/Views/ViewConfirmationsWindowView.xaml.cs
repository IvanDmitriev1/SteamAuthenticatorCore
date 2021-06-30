using System.Windows;
using System.Windows.Controls;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class ViewConfirmationsWindowView : Window
    {
        public ViewConfirmationsWindowView()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            listBox.SelectedItem = null;
        }

        private void ViewConfirmationsWindowView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataContext = (DataContext as ViewConfirmationsWindowViewModel)!;
            dataContext.WindowIsVisible = (bool) e.NewValue;
        }
    }
}
