using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Orders;
using MyShop.Models.Orders;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Services.Plugins;
using MyShop.Contracts;
using System;
using System.IO;
using System.Linq;

namespace MyShop.Views.Orders;

public sealed partial class CreateOrderPage : Page
{
    public CreateOrderViewModel ViewModel { get; }
    private readonly PluginLoader _pluginLoader;
    private IPaymentPlugin? _paymentPlugin;

    public CreateOrderPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CreateOrderViewModel>();
        _pluginLoader = new PluginLoader();
        
        // ViewModel.OrderCreated += OnOrderCreated; // No longer needed
        
        _ = ViewModel.InitializeAsync();
        LoadPaymentPlugin();
    }

    /// <summary>
    /// Load payment plugin from Plugins directory
    /// </summary>
    private void LoadPaymentPlugin()
    {
        try
        {
            var appDirectory = AppContext.BaseDirectory;
            var pluginsDirectory = Path.Combine(appDirectory, "Plugins");
            var pluginPath = Path.Combine(pluginsDirectory, "MomoPayment.dll");

            System.Diagnostics.Debug.WriteLine($"?? Looking for payment plugin at: {pluginPath}");

            if (!File.Exists(pluginPath))
            {
                System.Diagnostics.Debug.WriteLine("?? Payment plugin not found");
                
                // Show warning InfoBar
                PluginStatusInfoBar.Title = "Plugin Không Tìm Thấy";
                PluginStatusInfoBar.Message = "MomoPayment.dll không có trong thư mục Plugins. Chỉ hỗ trợ thanh toán tiền mặt.";
                PluginStatusInfoBar.Severity = InfoBarSeverity.Warning;
                PluginStatusInfoBar.IsOpen = true;
                
                return;
            }

            _paymentPlugin = _pluginLoader.LoadPlugin<IPaymentPlugin>(pluginPath);

            if (_paymentPlugin != null)
            {
                System.Diagnostics.Debug.WriteLine($"? Payment plugin loaded: {_paymentPlugin.Name}");
                System.Diagnostics.Debug.WriteLine($"   Description: {_paymentPlugin.Description}");
                
                // Subscribe to payment completion event
                _paymentPlugin.OnPaymentCompleted += OnPaymentCompleted;
                
                // Show QR payment button
                PayWithQRButton.Visibility = Visibility.Visible;
                
                // Show success InfoBar
                PluginStatusInfoBar.Title = "Plugin Sẵn Sàng";
                PluginStatusInfoBar.Message = $"{_paymentPlugin.Name} - {_paymentPlugin.Description}";
                PluginStatusInfoBar.Severity = InfoBarSeverity.Success;
                PluginStatusInfoBar.IsOpen = true;
                
                System.Diagnostics.Debug.WriteLine("? QR Payment button is now visible");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("? Failed to load plugin");
                
                // Show error InfoBar
                PluginStatusInfoBar.Title = "Lỗi Load Plugin";
                PluginStatusInfoBar.Message = "Không thể khởi tạo plugin thanh toán. Vui lòng kiểm tra log.";
                PluginStatusInfoBar.Severity = InfoBarSeverity.Error;
                PluginStatusInfoBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error loading payment plugin: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   Stack trace: {ex.StackTrace}");
            
            // Show error InfoBar
            PluginStatusInfoBar.Title = "Lỗi";
            PluginStatusInfoBar.Message = $"Lỗi khi load plugin: {ex.Message}";
            PluginStatusInfoBar.Severity = InfoBarSeverity.Error;
            PluginStatusInfoBar.IsOpen = true;
        }
    }

    /// <summary>
    /// Handle QR payment button click
    /// </summary>
    private async void PayWithQR_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasItems)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Giỏ Hàng Trống",
                Content = "Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
            return;
        }

        if (_paymentPlugin == null)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Lỗi Plugin",
                Content = "Plugin thanh toán chưa được load. Vui lòng sử dụng thanh toán tiền mặt.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
            return;
        }

        try
        {
            // Reset status
            ViewModel.IsPaymentCompleted = false;

            // Initialize payment plugin with current final amount
            _paymentPlugin.Initialize(ViewModel.FinalAmount);

            // Get payment UI from plugin
            var paymentView = _paymentPlugin.GetPaymentView();

            // Show payment dialog
            var paymentDialog = new ContentDialog
            {
                Title = "Thanh Toán QR (Momo)",
                Content = paymentView,
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            // If paymentView is a PaymentControl (from MomoPayment plugin), subscribe to DialogCloseRequested
            if (paymentView is UserControl uc && uc.GetType().GetEvent("DialogCloseRequested") != null)
            {
                var closeEvent = uc.GetType().GetEvent("DialogCloseRequested");
                EventHandler closeHandler = null!;
                closeHandler = (s, e) => {
                    paymentDialog.Hide();
                    closeEvent?.RemoveEventHandler(uc, closeHandler);
                };
                closeEvent?.AddEventHandler(uc, closeHandler);
            }

            System.Diagnostics.Debug.WriteLine($"Showing QR payment dialog for amount: {ViewModel.FinalAmount:N0} VNĐ");

            // Open the dialog and WAIT for user to close it or for payment to finish
            // Note: If payment is successful, the OnPaymentCompleted event will fire 
            // and update ViewModel.IsPaymentCompleted.
            await paymentDialog.ShowAsync();

            // Cleanup plugin resources
            _paymentPlugin.Cleanup();

            // Check if payment was successful
            if (ViewModel.IsPaymentCompleted)
            {
                ViewModel.IsLoading = true;
                
                // Now create the order in the database with "Paid" status
                // The ViewModel.CreateOrderCommand already uses IsPaymentCompleted to set Status=1
                await ViewModel.CreateOrderCommand.ExecuteAsync(null);
                
                System.Diagnostics.Debug.WriteLine("QR Order created successfully after payment confirmation.");
                
                // Navigate to Order Detail page after successful payment
                if (ViewModel.CreatedOrderId.HasValue)
                {
                    this.Frame.Navigate(typeof(OrderDetailPage), ViewModel.CreatedOrderId.Value);
                }
                else
                {
                    this.Frame.Navigate(typeof(OrdersListPage));
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("QR Payment was cancelled or failed. Order NOT created.");
                ViewModel.ErrorMessage = "Thanh toán chưa hoàn tất. Đơn hàng chưa được tạo.";
                ViewModel.HasError = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error in QR payment flow: {ex.Message}");
            
            var errorDialog = new ContentDialog
            {
                Title = "Lỗi Thanh Toán",
                Content = $"Không thể xử lý quy trình thanh toán QR: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            try { await errorDialog.ShowAsync(); } catch { }
        }
        finally
        {
            ViewModel.IsLoading = false;
        }
    }



    /// <summary>
    /// Handle payment completion from plugin
    /// </summary>
    private async void OnPaymentCompleted(object? sender, PaymentResult result)
    {
        // Run on UI thread
        DispatcherQueue.TryEnqueue(async () =>
        {
            var dialog = new ContentDialog
            {
                Title = result.IsSuccess ? "Thanh Toán Thành Công" : "Thanh Toán Thất Bại",
                Content = result.IsSuccess 
                    ? $"{result.Message}\n\nMã giao dịch: {result.TransactionId}"
                    : $"{result.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            try { await dialog.ShowAsync(); } catch { }

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"? Payment successful: {result.TransactionId}");
                ViewModel.IsPaymentCompleted = true; // Sync with ViewModel
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"? Payment failed: {result.Message}");
                ViewModel.IsPaymentCompleted = false;
            }
        });
    }

    // Removed OnOrderCreated as it's handled in ViewModel directly now.

    // Customer Search
    private async void CustomerSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = CustomerSearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length >= 1)
        {
            await ViewModel.SearchCustomersCommand.ExecuteAsync(searchText);
        }
        else
        {
            ViewModel.CustomerSuggestions.Clear();
        }
    }

    private void CustomerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Models.Customers.Customer customer)
        {
            ViewModel.SelectCustomerCommand.Execute(customer);
            CustomerListView.SelectedItem = null;
        }
    }

    // Product Search
    private async void ProductSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = ProductSearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchText) && searchText.Length >= 1)
        {
            await ViewModel.SearchProductsCommand.ExecuteAsync(searchText);
        }
        else
        {
            ViewModel.ProductSuggestions.Clear();
        }
    }

    private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Models.Products.Product product)
        {
            ViewModel.AddToCartCommand.Execute(product);
            ProductListView.SelectedItem = null;
            ProductSearchTextBox.Text = string.Empty;
        }
    }

    // Cart Management
    private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            ViewModel.RemoveFromCartCommand.Execute(item);
        }
    }

    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            item.Quantity++;
        }
    }

    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is CartItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back to Orders List
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
        else
        {
            this.Frame.Navigate(typeof(OrdersListPage));
        }
        ViewModel.ClearCartCommand.Execute(null);
        ViewModel.ClearCustomerCommand.Execute(null);
    }

    // Coupon Search
    private void CouponSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.FilterCoupons();
    }

    // Coupon RadioButton Selection
    private void CouponRadioButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && radioButton.Tag is AvailableCoupon coupon)
        {
            // Check if this coupon is already selected
            if (ViewModel.CouponCode == coupon.Code)
            {
                // If clicking the already selected coupon -> Deselect it
                ViewModel.ClearCouponCommand.Execute(null);
                radioButton.IsChecked = false;
            }
            else
            {
                // If clicking a new coupon -> Select it
                ViewModel.SelectCouponCommand.Execute(coupon);
            }
        }
    }
}

