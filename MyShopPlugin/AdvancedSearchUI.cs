using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace MyShopPlugin;

/// <summary>
/// Pure C# UI implementation for Advanced Search Plugin
/// Creates WinUI 3 controls programmatically without XAML
/// This class is part of the plugin DLL and can be dynamically loaded
/// </summary>
public class AdvancedSearchUI : UserControl
{
    private readonly ISearchPlugin _plugin;
    private readonly PluginUIViewModel _viewModel;

    // UI Controls (for event binding)
    private TextBox? _keywordTextBox;
    private ComboBox? _categoryComboBox;
    private NumberBox? _minPriceBox;
    private NumberBox? _maxPriceBox;

    /// <summary>
    /// Creates the UI with pure C# code
    /// </summary>
    /// <param name="plugin">The plugin instance providing the logic</param>
    public AdvancedSearchUI(ISearchPlugin plugin)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _viewModel = new PluginUIViewModel(_plugin);
        
        BuildUI();
    }

    /// <summary>
    /// Builds the entire UI hierarchy programmatically
    /// </summary>
    private void BuildUI()
    {
        // Main container
        var mainGrid = new Grid
        {
            Padding = new Thickness(16),
            RowSpacing = 12
        };

        // Define rows
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Keyword
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Category
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Price
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        var config = _plugin.GetFilterConfiguration();

        int currentRow = 0;

        // Row 0: Keyword Search
        if (config.SupportsKeywordSearch)
        {
            var keywordSection = CreateKeywordSection();
            Grid.SetRow(keywordSection, currentRow++);
            mainGrid.Children.Add(keywordSection);
        }

        // Row 1: Category Filter
        if (config.SupportsCategoryFilter)
        {
            var categorySection = CreateCategorySection(config);
            Grid.SetRow(categorySection, currentRow++);
            mainGrid.Children.Add(categorySection);
        }

        // Row 2: Price Range
        if (config.SupportsPriceRange)
        {
            var priceSection = CreatePriceSection();
            Grid.SetRow(priceSection, currentRow++);
            mainGrid.Children.Add(priceSection);
        }

        // Row 3: Action Buttons
        var buttonSection = CreateButtonSection();
        Grid.SetRow(buttonSection, currentRow);
        mainGrid.Children.Add(buttonSection);

        // Set as content
        Content = mainGrid;
    }

    /// <summary>
    /// Creates the keyword search section
    /// </summary>
    private StackPanel CreateKeywordSection()
    {
        var stack = new StackPanel { Spacing = 8 };

        // Label
        var label = new TextBlock
        {
            Text = "T? khóa tìm ki?m",
            FontWeight = FontWeights.SemiBold
        };
        stack.Children.Add(label);

        // TextBox with two-way binding
        _keywordTextBox = new TextBox
        {
            PlaceholderText = "Nh?p t? khóa tìm ki?m...",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var binding = new Binding
        {
            Source = _viewModel,
            Path = new PropertyPath(nameof(PluginUIViewModel.Keyword)),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        _keywordTextBox.SetBinding(TextBox.TextProperty, binding);

        stack.Children.Add(_keywordTextBox);

        return stack;
    }

    /// <summary>
    /// Creates the category filter section
    /// </summary>
    private StackPanel CreateCategorySection(PluginFilterConfiguration config)
    {
        var stack = new StackPanel { Spacing = 8 };

        // Label
        var label = new TextBlock
        {
            Text = "Lo?i s?n ph?m",
            FontWeight = FontWeights.SemiBold
        };
        stack.Children.Add(label);

        // ComboBox
        _categoryComboBox = new ComboBox
        {
            PlaceholderText = "Ch?n lo?i s?n ph?m",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberPath = "Name"
        };

        // Populate items directly (avoiding binding for simplicity in dynamic load scenario)
        foreach (var category in config.AvailableCategories)
        {
            _categoryComboBox.Items.Add(category);
        }

        // Set default selection
        if (_categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }

        // Handle selection changed event
        _categoryComboBox.SelectionChanged += (s, e) =>
        {
            if (_categoryComboBox.SelectedItem is CategoryOption selected)
            {
                _viewModel.SelectedCategoryId = selected.Id == 0 ? null : selected.Id;
            }
        };

        stack.Children.Add(_categoryComboBox);

        return stack;
    }

    /// <summary>
    /// Creates the price range section
    /// </summary>
    private StackPanel CreatePriceSection()
    {
        var stack = new StackPanel { Spacing = 8 };

        // Label
        var label = new TextBlock
        {
            Text = "Kho?ng giá",
            FontWeight = FontWeights.SemiBold
        };
        stack.Children.Add(label);

        // Price range grid
        var priceGrid = new Grid { ColumnSpacing = 12 };
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Min Price NumberBox
        _minPriceBox = new NumberBox
        {
            Header = "Giá t?i thi?u",
            PlaceholderText = "0",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            Minimum = 0,
            Maximum = 999999999
        };

        var minBinding = new Binding
        {
            Source = _viewModel,
            Path = new PropertyPath(nameof(PluginUIViewModel.MinPrice)),
            Mode = BindingMode.TwoWay
        };
        _minPriceBox.SetBinding(NumberBox.ValueProperty, minBinding);

        Grid.SetColumn(_minPriceBox, 0);
        priceGrid.Children.Add(_minPriceBox);

        // Separator
        var separator = new TextBlock
        {
            Text = "—",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0)
        };
        Grid.SetColumn(separator, 1);
        priceGrid.Children.Add(separator);

        // Max Price NumberBox
        _maxPriceBox = new NumberBox
        {
            Header = "Giá t?i ?a",
            PlaceholderText = "999,999,999",
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            Minimum = 0,
            Maximum = 999999999
        };

        var maxBinding = new Binding
        {
            Source = _viewModel,
            Path = new PropertyPath(nameof(PluginUIViewModel.MaxPrice)),
            Mode = BindingMode.TwoWay
        };
        _maxPriceBox.SetBinding(NumberBox.ValueProperty, maxBinding);

        Grid.SetColumn(_maxPriceBox, 2);
        priceGrid.Children.Add(_maxPriceBox);

        stack.Children.Add(priceGrid);

        return stack;
    }

    /// <summary>
    /// Creates the action buttons section
    /// </summary>
    private StackPanel CreateButtonSection()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 12, 0, 0)
        };

        // Filter Button (with accent style)
        var filterButton = new Button
        {
            Content = "L?c",
            MinWidth = 100
        };

        // Try to apply accent button style
        try
        {
            if (Application.Current?.Resources.TryGetValue("AccentButtonStyle", out var accentStyle) == true)
            {
                filterButton.Style = accentStyle as Style;
            }
        }
        catch
        {
            // Fallback: style not available
        }

        // Wire up click event
        filterButton.Click += OnFilterButtonClick;
        stack.Children.Add(filterButton);

        // Clear Button
        var clearButton = new Button
        {
            Content = "Xóa l?c",
            MinWidth = 100
        };

        clearButton.Click += OnClearButtonClick;
        stack.Children.Add(clearButton);

        return stack;
    }

    /// <summary>
    /// Handles the Filter button click
    /// </summary>
    private void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var filter = new SearchFilterArgs
            {
                Keyword = string.IsNullOrWhiteSpace(_viewModel.Keyword) ? null : _viewModel.Keyword.Trim(),
                CategoryId = _viewModel.SelectedCategoryId,
                MinPrice = _viewModel.MinPrice > 0 ? (decimal?)_viewModel.MinPrice : null,
                MaxPrice = _viewModel.MaxPrice > 0 ? (decimal?)_viewModel.MaxPrice : null
            };

            _plugin.ApplyFilter(filter);

            System.Diagnostics.Debug.WriteLine("? Filter applied from plugin UI");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Filter error: {ex.Message}");
            
            // Show error dialog if possible
            ShowErrorDialog($"L?i khi áp d?ng b? l?c: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Clear button click
    /// </summary>
    private void OnClearButtonClick(object sender, RoutedEventArgs e)
    {
        // Reset ViewModel
        _viewModel.Keyword = string.Empty;
        _viewModel.SelectedCategoryId = null;
        _viewModel.MinPrice = 0;
        _viewModel.MaxPrice = 0;

        // Reset ComboBox
        if (_categoryComboBox != null && _categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }

        // Clear plugin filter
        _plugin.ClearFilter();

        System.Diagnostics.Debug.WriteLine("? Filter cleared from plugin UI");
    }

    /// <summary>
    /// Shows an error dialog (async fire-and-forget)
    /// </summary>
    private async void ShowErrorDialog(string message)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "L?i",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch
        {
            // XamlRoot might not be set yet
            System.Diagnostics.Debug.WriteLine($"Cannot show dialog: {message}");
        }
    }
}

/// <summary>
/// Simple ViewModel for the plugin UI (internal to plugin)
/// </summary>
internal class PluginUIViewModel
{
    private readonly ISearchPlugin _plugin;

    public string Keyword { get; set; } = string.Empty;
    public int? SelectedCategoryId { get; set; }
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; }

    public PluginUIViewModel(ISearchPlugin plugin)
    {
        _plugin = plugin;
        
        // Load current filter state
        var currentFilter = _plugin.GetCurrentFilter();
        Keyword = currentFilter.Keyword ?? string.Empty;
        SelectedCategoryId = currentFilter.CategoryId;
        MinPrice = currentFilter.MinPrice.HasValue ? (double)currentFilter.MinPrice.Value : 0;
        MaxPrice = currentFilter.MaxPrice.HasValue ? (double)currentFilter.MaxPrice.Value : 0;
    }
}
