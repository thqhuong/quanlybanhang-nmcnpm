namespace quanlybanhang_nmcnpm
{
    public partial class DashboardWindow : System.Windows.Window
    {
        public DashboardWindow(string roleName)
        {
            InitializeComponent();
            txtRoleName.Text = roleName;
            txtTitle.Text = $"Hệ thống Quản lý Bán hàng v1.0 - [Chế độ: {roleName}]";
        }

        private void Logout_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
