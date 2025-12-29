using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views;

namespace MyShop;

/// <summary>
/// Main application window with NavigationView.
/// Provides navigation between different pages (Dashboard, ProductList, etc.)
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<MainWindowViewModel>();

        // Set window size
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

        // Navigate to Dashboard on startup
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(Dashboard));

        // Load product count after initialization
        _ = ViewModel.LoadProductCountAsync();
    }

    /// <summary>
    /// Handles navigation when user selects a menu item.
    /// </summary>
    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            return;
        }

        var selectedItem = args.SelectedItemContainer as NavigationViewItem;
        if (selectedItem == null) return;

        string tag = selectedItem.Tag?.ToString() ?? string.Empty;

        Type? pageType = tag switch
        {
            "Dashboard" => typeof(Dashboard),
            "ProductList" => typeof(ProductListPage),
            "AddProduct" => typeof(AddProductPage),
            "Categories" => null, // Not implemented yet
            _ => null
        };

        if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
