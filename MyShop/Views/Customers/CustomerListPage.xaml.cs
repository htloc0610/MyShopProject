using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models.Customers;
using MyShop.ViewModels.Customers;

namespace MyShop.Views.Customers
{
    /// <summary>
    /// Customer list page with master-detail layout.
    /// </summary>
    public sealed partial class CustomerListPage : Page
    {
        public CustomerViewModel ViewModel { get; }

        public CustomerListPage()
        {
            ViewModel = App.Current.Services.GetRequiredService<CustomerViewModel>();
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

        private async void EditCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCustomer == null) return;
            ViewModel.PrepareEditCustomerCommand.Execute(ViewModel.SelectedCustomer);
            await ShowCustomerDialogAsync("Sửa Thông Tin Khách Hàng");
        }

        private async void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedCustomer == null) return;

            var dialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = $"Bạn có chắc chắn muốn xóa khách hàng \"{ViewModel.SelectedCustomer.Name}\"?\n\nHành động này không thể hoàn tác.",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteCustomerCommand.ExecuteAsync(ViewModel.SelectedCustomer);
            }
        }

        private async System.Threading.Tasks.Task ShowCustomerDialogAsync(string title)
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
                ViewModel.FormName = nameBox.Text;
                ViewModel.FormPhoneNumber = phoneBox.Text;
                ViewModel.FormAddress = addressBox.Text;
                ViewModel.FormBirthday = birthdayPicker.Date;

                await ViewModel.SaveCustomerCommand.ExecuteAsync(null);
            }
        }
    }
}
