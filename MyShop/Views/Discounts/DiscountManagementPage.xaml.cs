using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Discounts;
using MyShop.Models.Discounts;

namespace MyShop.Views.Discounts;

/// <summary>
/// Page for managing discount codes.
/// </summary>
public sealed partial class DiscountManagementPage : Page
{
    public DiscountViewModel ViewModel { get; }

    public DiscountManagementPage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI
        ViewModel = App.Current.Services.GetRequiredService<DiscountViewModel>();
        
        // Load discounts on page load
        this.Loaded += DiscountManagementPage_Loaded;
    }

    private async void DiscountManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadDiscountsCommand.ExecuteAsync(null);
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        await DiscountDialog.ShowAsync();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Discount discount)
        {
            ViewModel.PrepareEditCommand.Execute(discount);
            await DiscountDialog.ShowAsync();
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Discount discount)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Discount",
                Content = $"Are you sure you want to delete the discount code '{discount.Code}'?",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteDiscountCommand.ExecuteAsync(discount);
            }
        }
    }



    private async void DiscountDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Prevent dialog from closing immediately (we'll close it after save)
        args.Cancel = true;

        await ViewModel.SaveDiscountCommand.ExecuteAsync(null);

        // Close dialog if save was successful (check for success via status message)
        if (!ViewModel.StatusMessage.StartsWith("Failed") && !ViewModel.StatusMessage.StartsWith("Error"))
        {
            sender.Hide();
        }
    }

    private void DiscountDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Just close the dialog
    }
}
