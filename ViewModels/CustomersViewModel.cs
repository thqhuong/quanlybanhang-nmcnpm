using System.Collections.ObjectModel;
using System.Windows;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class CustomersViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private string _searchText = "";
    private CustomerListItem? _selectedCustomer;
    private string _name = "";
    private string _phone = "";
    private string _email = "";
    private string _address = "";
    private string _pointsText = "";
    private DateTime? _birthDate;
    private string _statusMessage = "";

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        NewCommand = new RelayCommand(ClearForm);
    }

    public ObservableCollection<CustomerListItem> Customers { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public CustomerListItem? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                FillForm(value);
            }
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string PointsText
    {
        get => _pointsText;
        set => SetProperty(ref _pointsText, value);
    }

    public DateTime? BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int CustomerCount => Customers.Count;

    private async Task LoadAsync()
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var customers = await _customerService.SearchAsync(SearchText);
        Customers.ResetWith(customers);
        OnPropertyChanged(nameof(CustomerCount));
        StatusMessage = $"Tổng số khách hàng: {CustomerCount}";
    }

    private async Task AddAsync()
    {
        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var result = await _customerService.CreateAsync(input);
        StatusMessage = result.IsValid ? "Đã thêm khách hàng." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            await SearchAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedCustomer is null)
        {
            StatusMessage = "Vui lòng chọn khách hàng cần cập nhật.";
            return;
        }

        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var updatedId = SelectedCustomer.Id;
        var result = await _customerService.UpdateAsync(updatedId, input);
        StatusMessage = result.IsValid ? "Đã cập nhật khách hàng." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            await SearchAsync();
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == updatedId);
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedCustomer is null)
        {
            StatusMessage = "Vui lòng chọn khách hàng cần xóa.";
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa \"{SelectedCustomer.Name}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var deleted = SelectedCustomer;
        var result = await _customerService.DeleteAsync(deleted.Id);
        StatusMessage = result.IsValid ? "Đã xóa khách hàng." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            Customers.Remove(deleted);
            OnPropertyChanged(nameof(CustomerCount));
            StatusMessage = $"Đã xóa khách hàng. Tổng số khách hàng: {CustomerCount}";
        }
    }

    private CustomerInput? BuildInput()
    {
        if (!int.TryParse(PointsText, out var points))
        {
            StatusMessage = "Điểm tích lũy không hợp lệ.";
            return null;
        }

        return new CustomerInput(Name, Phone, Email, Address, BirthDate, points);
    }

    private void FillForm(CustomerListItem? customer)
    {
        if (customer is null)
        {
            return;
        }

        Name = customer.Name;
        Phone = customer.Phone;
        Email = customer.Email;
        Address = customer.Address;
        BirthDate = customer.BirthDate;
        PointsText = customer.Points.ToString();
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        Name = "";
        Phone = "";
        Email = "";
        Address = "";
        BirthDate = null;
        PointsText = "";
        StatusMessage = "";
    }
}
