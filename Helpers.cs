using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace TextureGroupsConfigurator
{
    public static class Helpers
    {
        public static object? GetRowItem(object sender)
        {
            var button = sender as Button;
            return button.Tag;
        }
        public static void RefreshGrid(DataGrid dg)
        {
            dg.CommitEdit(DataGridEditingUnit.Cell, true);
            dg.CommitEdit(DataGridEditingUnit.Row, true);
            CollectionViewSource.GetDefaultView(dg.ItemsSource).Refresh();
        }
    }
}
