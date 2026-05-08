using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace quanlybanhang_nmcnpm.Views
{
    public partial class AccountsControl : UserControl
    {
        public AccountsControl()
        {
            InitializeComponent();

            var accounts = new ObservableCollection<AccountModel>
            {
                new AccountModel { Id = 1, Username = "admin", FullName = "Trần Quản Trị", Role = "Quản trị viên (Admin)", Status = "Hoạt động", StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")), StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")), LastLogin = "2026-04-14 08:30" },
                new AccountModel { Id = 2, Username = "thungan01", FullName = "Nguyễn Thu Ngân", Role = "Thu ngân (Cashier)", Status = "Hoạt động", StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")), StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")), LastLogin = "2026-04-14 07:45" },
                new AccountModel { Id = 3, Username = "thungan02", FullName = "Lê Bán Hàng", Role = "Thu ngân (Cashier)", Status = "Tạm khóa", StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")), StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")), LastLogin = "2026-04-10 17:00" },
                new AccountModel { Id = 4, Username = "thukho_hn", FullName = "Phạm Thủ Kho", Role = "Thủ kho (Storekeeper)", Status = "Hoạt động", StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")), StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")), LastLogin = "2026-04-14 09:15" },
            };

            var listView = (ListView)Content.GetType().GetProperty("Children").GetValue(Content, null).GetType().GetMethod("get_Item").Invoke(Content.GetType().GetProperty("Children").GetValue(Content, null), new object[] { 3 });
            listView.ItemsSource = accounts;
        }
    }

    public class AccountModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public Brush StatusBackground { get; set; }
        public Brush StatusForeground { get; set; }
        public string LastLogin { get; set; }
    }
}