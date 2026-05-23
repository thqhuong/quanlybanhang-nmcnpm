using quanlybanhang_nmcnpm.Views;

namespace quanlybanhang_nmcnpm
{
    public partial class DashboardWindow : System.Windows.Window
    {
        public DashboardWindow(string roleName)
        {
            InitializeComponent();
            txtRoleName.Text = roleName;
            txtTitle.Text = $"Hệ thống Quản lý Bán hàng v1.0 - [Chế độ: {roleName}]";
            
            // Load default view
            MainContentControl.Content = new OverviewControl();
        }

        private void Menu_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.RadioButton rb && MainContentControl != null)
            {
                string tag = rb.Tag?.ToString();
                
                switch (tag)
                {
                    case "TongQuan":
                         MainContentControl.Content = new OverviewControl();
                        break;
                    case "BanHang":
                         MainContentControl.Content = new SalesControl();
                        break;
                    case "TaiKhoan":
                         MainContentControl.Content = new AccountsControl();
                        break;
                    case "HangHoa":
                         MainContentControl.Content = new ProductsControl();
                        break;
                    case "KhachHang":
                         MainContentControl.Content = new CustomersControl();
                        break;
                    case "PhieuNhap":
                         MainContentControl.Content = new ImportControl();
                        break;
                    // Add other cases as needed
                    default:
                        MainContentControl.Content = new System.Windows.Controls.TextBlock() { Text = "View in development", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize=24 };
                        break;
                }
            }
        }

        private void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
