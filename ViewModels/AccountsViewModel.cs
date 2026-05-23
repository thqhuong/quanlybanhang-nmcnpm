using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class AccountsViewModel : ViewModelBase
{
    private readonly IAccountService _accountService;
    private AccountListItem? _selectedAccount;
    private CategoryOption? _selectedRole;
    private string _username = "";
    private string _fullName = "";
    private string _phone = "";
    private string _email = "";
    private bool _isActive = true;
    private string _statusMessage = "";

    public AccountsViewModel(IAccountService accountService)
    {
        _accountService = accountService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync, () => SelectedAccount is not null);
        ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => SelectedAccount is not null);
        NewCommand = new RelayCommand(ClearForm);
    }

    public ObservableCollection<AccountListItem> Accounts { get; } = new();
    public ObservableCollection<CategoryOption> Roles { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public AsyncRelayCommand ToggleActiveCommand { get; }
    public RelayCommand NewCommand { get; }

    public AccountListItem? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (SetProperty(ref _selectedAccount, value))
            {
                FillForm(value);
                UpdateCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public CategoryOption? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
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

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadAsync()
    {
        Roles.ResetWith(await _accountService.GetRolesAsync());
        SelectedRole ??= Roles.FirstOrDefault();
        Accounts.ResetWith(await _accountService.GetAllAsync());
    }

    private async Task AddAsync()
    {
        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var result = await _accountService.CreateAsync(input);
        StatusMessage = result.IsValid ? "Đã thêm tài khoản." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            await LoadAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedAccount is null)
        {
            return;
        }

        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var result = await _accountService.UpdateAsync(SelectedAccount.Id, input);
        StatusMessage = result.IsValid ? "Đã cập nhật tài khoản." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            await LoadAsync();
        }
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedAccount is null)
        {
            return;
        }

        var result = await _accountService.SetActiveAsync(SelectedAccount.Id, !SelectedAccount.IsActive);
        StatusMessage = result.IsValid ? "Đã đổi trạng thái tài khoản." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            await LoadAsync();
        }
    }

    private AccountInput? BuildInput()
    {
        if (SelectedRole is null)
        {
            StatusMessage = "Vui lòng chọn vai trò.";
            return null;
        }

        return new AccountInput(Username, FullName, Phone, Email, SelectedRole.Id, IsActive);
    }

    private void FillForm(AccountListItem? account)
    {
        if (account is null)
        {
            return;
        }

        Username = account.Username;
        FullName = account.FullName;
        Phone = account.Phone;
        Email = account.Email;
        IsActive = account.IsActive;
        SelectedRole = Roles.FirstOrDefault(r => r.Name == account.Role) ?? SelectedRole;
    }

    private void ClearForm()
    {
        SelectedAccount = null;
        Username = "";
        FullName = "";
        Phone = "";
        Email = "";
        IsActive = true;
        StatusMessage = "";
    }
}
