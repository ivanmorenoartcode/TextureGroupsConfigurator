using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextureGroupsConfigurator
{
    /// <summary>
    /// Interaction logic for ScalabilityDataGrid.xaml
    /// </summary>
    public partial class ScalabilityDataGrid : UserControl
    {
        public ScalabilityDataGrid()
        {
            InitializeComponent();
        }

        private void Reset_CurrentValue(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            DataGrid dataGrid = FindParent<DataGrid>(button);
            ScalabilitySetting ss = Helpers.GetRowItem(sender) as ScalabilitySetting;
            ss.ResetValues();
            Helpers.RefreshGrid(dataGrid);
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
    }
}
