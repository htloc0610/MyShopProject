using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Products;

namespace MyShop.Views.Products;

/// <summary>
/// Host-rendered UI for Plugin filters - Created programmatically
/// Uses data binding to PluginFilterViewModel
/// </summary>
public sealed class PluginFilterUI : UserControl
{
    public PluginFilterViewModel ViewModel { get; }

    public PluginFilterUI(PluginFilterViewModel viewModel)
    {
        ViewModel = viewModel;
        CreateUI();
    }

    private void CreateUI()
    {
        var mainGrid = new Grid
        {
            Padding = new Thickness(16),
            RowSpacing = 12
        };

        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Row 0: Keyword Search
        if (ViewModel.SupportsKeywordSearch == Visibility.Visible)
        {
            var keywordStack = CreateKeywordSection();
            Grid.SetRow(keywordStack, 0);
            mainGrid.Children.Add(keywordStack);
        }

        // Row 1: Category
        if (ViewModel.SupportsCategoryFilter == Visibility.Visible)
        {
            var categoryStack = CreateCategorySection();
            Grid.SetRow(categoryStack, 1);
            mainGrid.Children.Add(categoryStack);
        }

        // Row 2: Price Range
        if (ViewModel.SupportsPriceRange == Visibility.Visible)
        {
            var priceStack = CreatePriceSection();
            Grid.SetRow(priceStack, 2);
            mainGrid.Children.Add(priceStack);
        }

        // Row 3: Buttons
        var buttonStack = CreateButtonSection();
        Grid.SetRow(buttonStack, 3);
        mainGrid.Children.Add(buttonStack);

        Content = mainGrid;
    }

    private StackPanel CreateKeywordSection()
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "T? khóa tìm ki?m",
            FontWeight = FontWeights.SemiBold
        });

        var textBox = new TextBox
        {
            PlaceholderText = "Nh?p t? khóa tìm ki?m..."
        };

        // Bind to ViewModel
        var binding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.Keyword)),
            Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay,
            UpdateSourceTrigger = Microsoft.UI.Xaml.Data.UpdateSourceTrigger.PropertyChanged
        };
        textBox.SetBinding(TextBox.TextProperty, binding);

        stack.Children.Add(textBox);
        return stack;
    }

    private StackPanel CreateCategorySection()
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "Lo?i s?n ph?m",
            FontWeight = FontWeights.SemiBold
        });

        var comboBox = new ComboBox
        {
            PlaceholderText = "Ch?n lo?i s?n ph?m",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberPath = "Name"
        };

        // Bind ItemsSource
        var itemsBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.Categories)),
            Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay
        };
        comboBox.SetBinding(ComboBox.ItemsSourceProperty, itemsBinding);

        // Bind SelectedItem
        var selectedBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.SelectedCategory)),
            Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
        };
        comboBox.SetBinding(ComboBox.SelectedItemProperty, selectedBinding);

        stack.Children.Add(comboBox);
        return stack;
    }

    private StackPanel CreatePriceSection()
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock
        {
            Text = "Kho?ng giá",
            FontWeight = FontWeights.SemiBold
        });

        var priceGrid = new Grid { ColumnSpacing = 12 };
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Min Price
        var minPrice = new NumberBox
        {
            Header = "Giá t?i thi?u",
            PlaceholderText = "0",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };

        var minBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.MinPrice)),
            Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
        };
        minPrice.SetBinding(NumberBox.ValueProperty, minBinding);

        Grid.SetColumn(minPrice, 0);
        priceGrid.Children.Add(minPrice);

        // Separator
        var separator = new TextBlock
        {
            Text = "—",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0)
        };
        Grid.SetColumn(separator, 1);
        priceGrid.Children.Add(separator);

        // Max Price
        var maxPrice = new NumberBox
        {
            Header = "Giá t?i ?a",
            PlaceholderText = "999,999,999",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };

        var maxBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.MaxPrice)),
            Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
        };
        maxPrice.SetBinding(NumberBox.ValueProperty, maxBinding);

        Grid.SetColumn(maxPrice, 2);
        priceGrid.Children.Add(maxPrice);

        stack.Children.Add(priceGrid);
        return stack;
    }

    private StackPanel CreateButtonSection()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 12, 0, 0)
        };

        var filterButton = new Button
        {
            Content = "L?c",
            MinWidth = 100
        };

        var filterBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.ApplyFilterCommand))
        };
        filterButton.SetBinding(Button.CommandProperty, filterBinding);

        // Try to apply accent style
        try
        {
            if (Application.Current.Resources.TryGetValue("AccentButtonStyle", out var style))
            {
                filterButton.Style = style as Style;
            }
        }
        catch { }

        var clearButton = new Button
        {
            Content = "Xóa l?c",
            MinWidth = 100
        };

        var clearBinding = new Microsoft.UI.Xaml.Data.Binding
        {
            Source = ViewModel,
            Path = new PropertyPath(nameof(ViewModel.ClearFilterCommand))
        };
        clearButton.SetBinding(Button.CommandProperty, clearBinding);

        stack.Children.Add(filterButton);
        stack.Children.Add(clearButton);

        return stack;
    }
}
