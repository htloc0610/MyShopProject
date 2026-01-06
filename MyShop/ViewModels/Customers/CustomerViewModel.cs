using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Customers;
using MyShop.Services.Customers;

namespace MyShop.ViewModels.Customers
{
    /// <summary>
    /// ViewModel for managing customer list with search and CRUD operations.
    /// </summary>
    public partial class CustomerViewModel : ObservableObject
    {
        #region Fields

        private readonly ICustomerService _customerService;

        #endregion

        #region Observable Properties - Display State

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        [ObservableProperty]
        private ObservableCollection<Customer> _filteredCustomers = new();

        [ObservableProperty]
        private Customer? _selectedCustomer;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private int _customerCount;

        [ObservableProperty]
        private bool _isDetailPaneVisible;

        #endregion

        #region Observable Properties - Paging

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _pageSize = 20;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
        private int _totalPages;

        public string PaginationInfo => $"Trang {CurrentPage} trên {TotalPages}";

        #endregion

        #region Observable Properties - Search/Filter

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        #endregion

        #region Observable Properties - Add/Edit Form

        [ObservableProperty]
        private string _formName = string.Empty;

        [ObservableProperty]
        private string _formPhoneNumber = string.Empty;

        [ObservableProperty]
        private string _formAddress = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? _formBirthday;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private Guid _editingCustomerId;

        #endregion

        #region Constructor

        public CustomerViewModel(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize and load initial data.
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadCustomersAsync();
        }

        #endregion

        #region Commands - Data Loading

        /// <summary>
        /// Load customers from API with current paging and search settings.
        /// </summary>
        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var result = await _customerService.GetCustomersAsync(
                    CurrentPage,
                    PageSize,
                    SearchKeyword);

                Customers.Clear();
                FilteredCustomers.Clear();

                foreach (var customer in result.Items)
                {
                    Customers.Add(customer);
                    FilteredCustomers.Add(customer);
                }

                CustomerCount = result.TotalCount;
                TotalPages = result.TotalPages;
                OnPropertyChanged(nameof(PaginationInfo));

                // Auto-select first customer if available and no selection
                if (SelectedCustomer == null && FilteredCustomers.Count > 0)
                {
                    SelectedCustomer = FilteredCustomers[0];
                }

                if (CustomerCount == 0 && string.IsNullOrEmpty(SearchKeyword))
                {
                    HasError = true;
                    ErrorMessage = "Chưa có khách hàng nào. Hãy thêm khách hàng mới!";
                }
            }
            catch (System.Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi khi tải khách hàng: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in LoadCustomersAsync: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadCustomersAsync();
        }

        #endregion

        #region Commands - Search

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadCustomersAsync();
        }

        [RelayCommand]
        private async Task ClearSearchAsync()
        {
            SearchKeyword = string.Empty;
            CurrentPage = 1;
            await LoadCustomersAsync();
        }

        partial void OnSearchKeywordChanged(string value)
        {
            // If empty, reload immediately
            if (string.IsNullOrEmpty(value))
            {
                _ = LoadCustomersAsync();
            }
        }

        #endregion

        #region Commands - Paging

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task GoToNextPageAsync()
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }

        private bool CanGoToNextPage()
        {
            return TotalPages > 0 && CurrentPage < TotalPages && !IsLoading;
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task GoToPreviousPageAsync()
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }

        private bool CanGoToPreviousPage()
        {
            return TotalPages > 0 && CurrentPage > 1 && !IsLoading;
        }

        #endregion

        #region Commands - CRUD Operations

        [RelayCommand]
        private void PrepareAddCustomer()
        {
            IsEditing = false;
            EditingCustomerId = Guid.Empty;
            FormName = string.Empty;
            FormPhoneNumber = string.Empty;
            FormAddress = string.Empty;
            FormBirthday = null;
        }

        [RelayCommand]
        private void PrepareEditCustomer(Customer? customer)
        {
            if (customer == null) return;

            IsEditing = true;
            EditingCustomerId = customer.Id;
            FormName = customer.Name;
            FormPhoneNumber = customer.PhoneNumber;
            FormAddress = customer.Address ?? string.Empty;
            FormBirthday = customer.Birthday.HasValue 
                ? new DateTimeOffset(customer.Birthday.Value) 
                : null;
        }

        [RelayCommand]
        private async Task SaveCustomerAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;

                if (string.IsNullOrWhiteSpace(FormName))
                {
                    HasError = true;
                    ErrorMessage = "Vui lòng nhập tên khách hàng";
                    return;
                }

                if (string.IsNullOrWhiteSpace(FormPhoneNumber))
                {
                    HasError = true;
                    ErrorMessage = "Vui lòng nhập số điện thoại";
                    return;
                }

                if (IsEditing)
                {
                    // Update existing customer
                    var existingCustomer = SelectedCustomer;
                    var updateDto = new CustomerUpdateDto
                    {
                        Id = EditingCustomerId,
                        Name = FormName,
                        PhoneNumber = FormPhoneNumber,
                        Address = string.IsNullOrWhiteSpace(FormAddress) ? null : FormAddress,
                        Birthday = FormBirthday?.DateTime,
                        TotalSpent = existingCustomer?.TotalSpent ?? 0
                    };

                    var updated = await _customerService.UpdateCustomerAsync(EditingCustomerId, updateDto);
                    if (updated != null)
                    {
                        await LoadCustomersAsync();
                        // Reselect the updated customer
                        SelectedCustomer = FilteredCustomers.FirstOrDefault(c => c.Id == EditingCustomerId);
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = "Không thể cập nhật khách hàng. Vui lòng thử lại.";
                    }
                }
                else
                {
                    // Create new customer
                    var createDto = new CustomerCreateDto
                    {
                        Name = FormName,
                        PhoneNumber = FormPhoneNumber,
                        Address = string.IsNullOrWhiteSpace(FormAddress) ? null : FormAddress,
                        Birthday = FormBirthday?.DateTime
                    };

                    var created = await _customerService.CreateCustomerAsync(createDto);
                    if (created != null)
                    {
                        CurrentPage = 1;
                        await LoadCustomersAsync();
                        SelectedCustomer = FilteredCustomers.FirstOrDefault(c => c.Id == created.Id);
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = "Không thể thêm khách hàng. Số điện thoại có thể đã tồn tại.";
                    }
                }
            }
            catch (System.Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error saving customer: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCustomerAsync(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                IsLoading = true;

                var success = await _customerService.DeleteCustomerAsync(customer.Id);

                if (success)
                {
                    Customers.Remove(customer);
                    FilteredCustomers.Remove(customer);
                    CustomerCount--;
                    OnPropertyChanged(nameof(PaginationInfo));

                    if (SelectedCustomer?.Id == customer.Id)
                    {
                        SelectedCustomer = FilteredCustomers.FirstOrDefault();
                    }

                    // Reload if current page is empty
                    if (FilteredCustomers.Count == 0 && CurrentPage > 1)
                    {
                        CurrentPage--;
                        await LoadCustomersAsync();
                    }
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Không thể xóa khách hàng. Vui lòng thử lại.";
                }
            }
            catch (System.Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi khi xóa: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Property Changed Handlers

        partial void OnSelectedCustomerChanged(Customer? value)
        {
            IsDetailPaneVisible = value != null;
        }

        #endregion
    }
}
