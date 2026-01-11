using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels.Orders;
using MyShop.Services.Orders;
using MyShop.Services.Shared;
using MyShop.Services.Auth;
using System;
using System.Threading.Tasks;
using WinRT;

namespace MyShop.Views.Orders;

public sealed partial class OrderDetailPage : Page
{
    public OrderDetailViewModel ViewModel { get; }

    public OrderDetailPage()
    {
        this.InitializeComponent();
        
        // Get services and initialize ViewModel (Frame will be set later)
        var orderService = App.Current.Services.GetService(typeof(IOrderService)) as IOrderService;
        var printService = App.Current.Services.GetService(typeof(IPrintService)) as IPrintService;
        var sessionService = App.Current.Services.GetService(typeof(ISessionService)) as ISessionService;
        ViewModel = new OrderDetailViewModel(orderService!, printService!, sessionService!, null);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Set the navigation frame now that the page is in the visual tree
        if (ViewModel is OrderDetailViewModel vm)
        {
            typeof(OrderDetailViewModel).GetField("_navigationFrame", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(vm, this.Frame);
        }
        
        if (e.Parameter is int orderId)
        {
            await ViewModel.LoadOrderAsync(orderId);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentOrder == null) return;

        var dialog = new ContentDialog
        {
            Title = "Xác nhận xóa",
            Content = $"Bạn chắc chắn muốn xóa đơn hàng #{ViewModel.CurrentOrder.OrderId}?\nĐơn hàng này không thể hoàn tác.",
            PrimaryButtonText = "Xóa",
            CloseButtonText = "Hủy",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteOrderCommand.ExecuteAsync(null);
        }
    }
}
