using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace quanlybanhang_nmcnpm.Views
{
    public partial class CustomersControl : UserControl
    {
        public CustomersControl()
        {
            InitializeComponent();

            var customers = new ObservableCollection<CustomerModel>
            {
                new CustomerModel { Id = 1, Phone = "0901234567", Name = "Nguyễn Văn A", Points = 150, LastPurchase = "2026-05-01" },
                new CustomerModel { Id = 2, Phone = "0912345678", Name = "Trần Thị B", Points = 25, LastPurchase = "2026-05-05" },
                new CustomerModel { Id = 3, Phone = "0987654321", Name = "Lê Văn C", Points = 500, LastPurchase = "2026-04-20" },
                new CustomerModel { Id = 4, Phone = "0977111222", Name = "Phạm Thu D", Points = 0, LastPurchase = "2026-05-08" }
            };

            if (FindName("CustomersListView") is ListView listView)
            {
                listView.ItemsSource = customers;
            }
        }
    }

    public class CustomerModel
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public string LastPurchase { get; set; }
    }
}