using System.Windows;
using System.Windows.Controls;

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
    }
}
