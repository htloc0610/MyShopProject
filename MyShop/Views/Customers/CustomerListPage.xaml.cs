using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models.Customers;
using MyShop.Services.Shared;
using MyShop.ViewModels.Customers;

namespace MyShop.Views.Customers
{
    /// <summary>
    /// Customer list page with DataGrid layout.
    /// </summary>
    public sealed partial class CustomerListPage : Page
    {
        public CustomerViewModel ViewModel { get; }
        private readonly IToastService _toastService;

        public CustomerListPage()
        {
            ViewModel = App.Current.Services.GetRequiredService<CustomerViewModel>();
            _toastService = App.Current.Services.GetRequiredService<IToastService>();
            InitializeComponent();
            Loaded += CustomerListPage_Loaded;
        }

        private async void CustomerListPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PrepareAddCustomerCommand.Execute(null);
            await ShowCustomerDialogAsync("Thêm Khách Hàng Mới");
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Customer customer)
            {
                ViewModel.PrepareEditCustomerCommand.Execute(customer);
                await ShowCustomerDialogAsync("Sửa Thông Tin Khách Hàng");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Customer customer)
            {
                var dialog = new ContentDialog
                {
                    Title = "Xác nhận xóa",
                    Content = $"Bạn có chắc chắn muốn xóa khách hàng \"{customer.Name}\"?\n\nHành động này không thể hoàn tác.",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteCustomerCommand.ExecuteAsync(customer);
                }
            }
        }

        private async Task ShowCustomerDialogAsync(string title)
        {
            var nameBox = new TextBox
            {
                Header = "Tên khách hàng *",
                PlaceholderText = "Nhập tên khách hàng",
                Text = ViewModel.FormName,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var phoneBox = new TextBox
            {
                Header = "Số điện thoại *",
                PlaceholderText = "Nhập số điện thoại",
                Text = ViewModel.FormPhoneNumber,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var addressBox = new TextBox
            {
                Header = "Địa chỉ",
                PlaceholderText = "Nhập địa chỉ (không bắt buộc)",
                Text = ViewModel.FormAddress,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 80,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var birthdayPicker = new CalendarDatePicker
            {
                Header = "Ngày sinh",
                PlaceholderText = "Chọn ngày sinh (không bắt buộc)",
                Date = ViewModel.FormBirthday,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var panel = new StackPanel
            {
                Width = 400
            };
            panel.Children.Add(nameBox);
            panel.Children.Add(phoneBox);
            panel.Children.Add(addressBox);
            panel.Children.Add(birthdayPicker);

            var dialog = new ContentDialog
            {
                Title = title,
                Content = panel,
                PrimaryButtonText = ViewModel.IsEditing ? "Cập nhật" : "Thêm",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Client-side validation
                var name = nameBox.Text?.Trim();
                var phone = phoneBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    _toastService.ShowWarning("Vui lòng nhập tên khách hàng");
                    return;
                }

                if (name.Length < 2)
                {
                    _toastService.ShowWarning("Tên khách hàng phải có ít nhất 2 ký tự");
                    return;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    _toastService.ShowWarning("Vui lòng nhập số điện thoại");
                    return;
                }

                // Validate phone number format (Vietnamese phone: 10 digits starting with 0)
                if (!Regex.IsMatch(phone, @"^0\d{9}$"))
                {
                    _toastService.ShowWarning("Số điện thoại không hợp lệ (10 chữ số, bắt đầu bằng 0)");
                    return;
                }

                // Validate birthday if provided
                if (birthdayPicker.Date.HasValue && birthdayPicker.Date.Value.Date > DateTime.Today)
                {
                    _toastService.ShowWarning("Ngày sinh không thể là ngày trong tương lai");
                    return;
                }

                ViewModel.FormName = name;
                ViewModel.FormPhoneNumber = phone;
                ViewModel.FormAddress = addressBox.Text?.Trim() ?? string.Empty;
                ViewModel.FormBirthday = birthdayPicker.Date;

                await ViewModel.SaveCustomerCommand.ExecuteAsync(null);
            }
        }
    }
}
