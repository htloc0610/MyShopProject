using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml; // For UIElement
using MyShop.Contracts;
using MyShop.Services.Plugins;
using Microsoft.Extensions.DependencyInjection; // For Service Locator

namespace MyShop.ViewModels.Orders;

public partial class CreateOrderViewModel
{
    [ObservableProperty]
    private ObservableCollection<IPaymentPlugin> _paymentPlugins = new();

    [ObservableProperty]
    private IPaymentPlugin? _selectedPaymentPlugin;

    [ObservableProperty]
    private UIElement? _paymentView;

    [ObservableProperty]
    private bool _isPaymentPluginSelected;

    [ObservableProperty]
    private bool _isPaymentCompleted;

    partial void OnSelectedPaymentPluginChanged(IPaymentPlugin? value)
    {
        IsPaymentPluginSelected = value != null;
        IsPaymentCompleted = false;

        if (value != null)
        {
            // Initialize with current amount
            value.Initialize(FinalAmount);
            value.OnPaymentCompleted += Plugin_OnPaymentCompleted;
            PaymentView = value.GetPaymentView();
        }
        else
        {
            PaymentView = null;
        }
    }

    /// <summary>
    /// Update payment plugin when amount changes
    /// </summary>
    partial void OnFinalAmountChanged(decimal value)
    {
        if (SelectedPaymentPlugin != null && !IsPaymentCompleted)
        {
            SelectedPaymentPlugin.Initialize(value);
            PaymentView = SelectedPaymentPlugin.GetPaymentView();
        }
    }

    private void Plugin_OnPaymentCompleted(object? sender, PaymentResult e)
    {
        if (e.IsSuccess)
        {
            IsPaymentCompleted = true;
            // Optionally auto-trigger create order?
            // For now, let user click "Create Order"
        }
        else
        {
            IsPaymentCompleted = false;
        }
    }

    public async Task LoadPaymentPluginsAsync()
    {
        try
        {
            // Use Service Locator to get PluginLoader (avoid constructor changes)
            var pluginLoader = ((App)Application.Current).Services.GetService<PluginLoader>();
            
            if (pluginLoader != null)
            {
                string pluginsDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Plugins");
                
                // Wrap synchronous load in Task.Run to simulate async/avoid UI block
                var plugins = await Task.Run(() => pluginLoader.LoadPlugins<IPaymentPlugin>(pluginsDir));
                
                PaymentPlugins.Clear();
                foreach (var plugin in plugins)
                {
                    PaymentPlugins.Add(plugin);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading payment plugins: {ex.Message}");
        }
    }
}
