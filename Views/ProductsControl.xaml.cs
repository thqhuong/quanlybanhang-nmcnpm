using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace quanlybanhang_nmcnpm.Views
{
    public partial class ProductsControl : UserControl
    {
        public ProductsControl()
        {
            InitializeComponent();

            var products = new ObservableCollection<ProductModel>
            {
                new ProductModel { Id = 1, Code = "SP001", Name = "Bánh quy bơ Danisa 454g", Category = "Bánh kẹo", Unit = "Hộp", Price = "150.000", Stock = "45", StockColor = new SolidColorBrush(Colors.Green) },
                new ProductModel { Id = 2, Code = "SP002", Name = "Sữa tươi TH True Milk 1L", Category = "Sữa", Unit = "Hộp", Price = "35.000", Stock = "120", StockColor = new SolidColorBrush(Colors.Green) },
                new ProductModel { Id = 3, Code = "SP003", Name = "Kẹo dẻo Chupa Chups", Category = "Bánh kẹo", Unit = "Gói", Price = "25.000", Stock = "80", StockColor = new SolidColorBrush(Colors.Green) },
                new ProductModel { Id = 4, Code = "SP004", Name = "Nước ngọt Coca Cola 1.5L", Category = "Nước giải khát", Unit = "Chai", Price = "20.000", Stock = "50", StockColor = new SolidColorBrush(Colors.Green) },
                new ProductModel { Id = 5, Code = "SP005", Name = "Mì Hảo Hảo Tôm Chua Cay", Category = "Mì ăn liền", Unit = "Thùng", Price = "135.000", Stock = "15", StockColor = new SolidColorBrush(Colors.Red) }
            };

            var listView = (ListView)Content.GetType().GetProperty("Children").GetValue(Content, null).GetType().GetMethod("get_Item").Invoke(Content.GetType().GetProperty("Children").GetValue(Content, null), new object[] { 3 });
            listView.ItemsSource = products;
        }
    }

    public class ProductModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public string Price { get; set; }
        public string Stock { get; set; }
        public Brush StockColor { get; set; }
    }
}