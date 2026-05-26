using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace quanlybanhang_nmcnpm.Views
{
    public partial class ImportControl : UserControl
    {
        public ImportControl()
        {
            InitializeComponent();

            var items = new ObservableCollection<ImportItemModel>
            {
                new ImportItemModel { Code = "SP001", Name = "Bánh quy bơ Danisa 454g", Unit = "Hộp", Quantity = 50, Price = "120.000", Total = "6.000.000" }
            };

            // Using x:Name instead of hardcoded child traversal
            var listView = FindName("ImportListView") as ListView;
            if (listView != null)
            {
                listView.ItemsSource = items;
            }
        }
    }

    public class ImportItemModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public string Total { get; set; }
    }
}