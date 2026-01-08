using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Contracts;
using MyShop.Contracts.Search;

namespace MyShopPlugin;

/// <summary>
/// Compact UI for Advanced Search Plugin - Maximum space efficiency
/// </summary>
public class AdvancedSearchUI : UserControl
{
    private readonly ISearchPlugin _plugin;
    private readonly PluginUIViewModel _viewModel;

    // UI Controls
    private TextBox? _keywordTextBox;
    private ComboBox? _categoryComboBox;
    private NumberBox? _minPriceBox;
    private NumberBox? _maxPriceBox;
    private CheckBox? _fuzzySearchCheckBox;
    private Slider? _fuzzyThresholdSlider;

    public AdvancedSearchUI(ISearchPlugin plugin)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _viewModel = new PluginUIViewModel(_plugin);
        
        BuildCompactUI();
    }

    /// <summary>
    /// Ultra-compact 3-row layout
    /// </summary>
    private void BuildCompactUI()
    {
        var mainGrid = new Grid
        {
            Padding = new Thickness(8),
            RowSpacing = 8
        };

        // 2 rows layout (Ultra Compact)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 0: Keyword + Fuzzy
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1: Category + Price + Buttons

        var config = _plugin.GetFilterConfiguration();

        // Row 0: Keyword + Fuzzy Search
        var row0 = CreateKeywordAndFuzzyRow(config);
        Grid.SetRow(row0, 0);
        mainGrid.Children.Add(row0);

        // Row 1: Category + Price + Buttons
        var row1 = CreateBottomRow(config);
        Grid.SetRow(row1, 1);
        mainGrid.Children.Add(row1);

        Content = mainGrid;
    }

    /// <summary>
    /// Row 0: [Keyword (Stretch)] | [Fuzzy (Auto)]
    /// </summary>
    /// <summary>
    /// Row 0: [Keyword (Stretch)] | [Fuzzy (Auto)]
    /// </summary>
    private Grid CreateKeywordAndFuzzyRow(PluginFilterConfiguration config)
    {
        var grid = new Grid { ColumnSpacing = 12 };
        
        // Col 0: Keyword (*)
        // Col 1: Fuzzy (Auto) - Packs tightly
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Column 0: Keyword TextBox (No Label - use Header)
        _keywordTextBox = new TextBox
        {
            Header = "Từ khóa",
            PlaceholderText = "Tìm kiếm...",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var keywordBinding = new Binding
        {
            Source = _viewModel,
            Path = new PropertyPath(nameof(PluginUIViewModel.Keyword)),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        _keywordTextBox.SetBinding(TextBox.TextProperty, keywordBinding);

        Grid.SetColumn(_keywordTextBox, 0);
        grid.Children.Add(_keywordTextBox);

        // Column 1: Fuzzy search controls
        if (config.EnableFuzzySearch)
        {
            var fuzzyStack = new StackPanel 
            { 
                Spacing = 2,
                VerticalAlignment = VerticalAlignment.Bottom, // Align with textbox input area
                Margin = new Thickness(0, 0, 0, 4) // Fine tune
            };

            // Using tooltip instead of Label to save space
            var fuzzyControls = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };
            ToolTipService.SetToolTip(fuzzyControls, "Tìm kiếm mờ (Fuzzy Search)");

            _fuzzySearchCheckBox = new CheckBox
            {
                Content = "Mờ",
                IsChecked = true,
                FontSize = 12,
                MinWidth = 0,
                VerticalAlignment = VerticalAlignment.Center
            };

            fuzzyControls.Children.Add(_fuzzySearchCheckBox);

            _fuzzyThresholdSlider = new Slider
            {
                Minimum = 50,
                Maximum = 95,
                Value = config.FuzzySearchThreshold,
                StepFrequency = 5,
                Width = 80,
                VerticalAlignment = VerticalAlignment.Center
            };

            var thresholdLabel = new TextBlock
            {
                Text = $"{config.FuzzySearchThreshold}%",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 30,
                HorizontalTextAlignment = TextAlignment.Right
            };

            _fuzzyThresholdSlider.ValueChanged += (s, e) =>
            {
                thresholdLabel.Text = $"{(int)e.NewValue}%";
            };

            _fuzzySearchCheckBox.Checked += (s, e) => _fuzzyThresholdSlider.IsEnabled = true;
            _fuzzySearchCheckBox.Unchecked += (s, e) => _fuzzyThresholdSlider.IsEnabled = false;

            fuzzyControls.Children.Add(_fuzzyThresholdSlider);
            fuzzyControls.Children.Add(thresholdLabel);

            fuzzyStack.Children.Add(fuzzyControls);
            Grid.SetColumn(fuzzyStack, 1);
            grid.Children.Add(fuzzyStack);
        }

        return grid;
    }

    /// <summary>
    /// Row 1: [Category (Fixed)] [Price (Auto)] [Spacer] [Buttons (Right)]
    /// </summary>
    private Grid CreateBottomRow(PluginFilterConfiguration config)
    {
        var grid = new Grid { ColumnSpacing = 12 };
        
        // Col 0: Category (Compact - 140px)
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
        // Col 1: Price (Auto - sized to content)
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        // Col 2: Spacer (*) - Pushes buttons to right
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        // Col 3: Buttons (Auto)
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // --- Column 0: Category ---
        // Header property is cleaner than separate TextBlock
        _categoryComboBox = new ComboBox
        {
            Header = "Loại",
            PlaceholderText = "Tất cả",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberPath = "Name"
        };

        foreach (var category in config.AvailableCategories)
        {
            _categoryComboBox.Items.Add(category);
        }

        if (_categoryComboBox.Items.Count > 0) _categoryComboBox.SelectedIndex = 0;

        _categoryComboBox.SelectionChanged += (s, e) =>
        {
            if (_categoryComboBox.SelectedItem is CategoryOption selected)
            {
                _viewModel.SelectedCategoryId = selected.Id == 0 ? null : selected.Id;
            }
        };

        Grid.SetColumn(_categoryComboBox, 0);
        grid.Children.Add(_categoryComboBox);

        // --- Column 1: Price ---
        // Container for Min/Max
        var priceStack = new StackPanel 
        { 
            Spacing = 4, 
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0,0,0,1)
        };
        // Label for group? "Giá" -> optional. 
        // Or integrate into NumberBox Header? 
        // If we use Header for Min/Max, it aligns with Category Header.
        
        var priceGrid = new Grid { ColumnSpacing = 8 };
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Compact Inputs
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        priceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

        _minPriceBox = new NumberBox
        {
            Header = "Giá từ",
            PlaceholderText = "Min",
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

        var separator = new TextBlock 
        { 
            Text = "-", 
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 16, 0, 0) // Align with input box content, ignoring header height
        };
        Grid.SetColumn(separator, 1);
        priceGrid.Children.Add(separator);

        _maxPriceBox = new NumberBox
        {
            Header = "Đến",
            PlaceholderText = "Max",
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

        Grid.SetColumn(priceGrid, 1);
        grid.Children.Add(priceGrid);

        // --- Column 3: Buttons (Row aligned with inputs) ---
        // VerticalAlignment = Bottom to match inputs
        var buttonStack = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Bottom, 
            Margin = new Thickness(0, 0, 0, 2) // Fine tune alignment
        };

        var filterButton = new Button
        {
            Content = "Lọc",
            MinWidth = 80,
            Style = Application.Current?.Resources["AccentButtonStyle"] as Style
        };
        filterButton.Click += OnFilterButtonClick;
        buttonStack.Children.Add(filterButton);

        var clearButton = new Button
        {
            Content = "Xóa",
            MinWidth = 70
        };
        clearButton.Click += OnClearButtonClick;
        buttonStack.Children.Add(clearButton);

        Grid.SetColumn(buttonStack, 3);
        grid.Children.Add(buttonStack);

        return grid;
    }

    private void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var filter = new SearchFilterArgs
            {
                Keyword = string.IsNullOrWhiteSpace(_viewModel.Keyword) ? null : _viewModel.Keyword.Trim(),
                CategoryId = _viewModel.SelectedCategoryId,
                MinPrice = _viewModel.MinPrice > 0 ? (decimal?)_viewModel.MinPrice : null,
                MaxPrice = _viewModel.MaxPrice > 0 ? (decimal?)_viewModel.MaxPrice : null,
                UseFuzzySearch = _fuzzySearchCheckBox?.IsChecked == true,
                FuzzyThreshold = _fuzzyThresholdSlider != null ? (int)_fuzzyThresholdSlider.Value : 70
            };

            _plugin.ApplyFilter(filter);

            System.Diagnostics.Debug.WriteLine("✅ Filter applied from plugin UI");
            System.Diagnostics.Debug.WriteLine($"   Fuzzy: {filter.UseFuzzySearch}, Threshold: {filter.FuzzyThreshold}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Filter error: {ex.Message}");
            ShowErrorDialog($"Lỗi: {ex.Message}");
        }
    }

    private void OnClearButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.Keyword = string.Empty;
        _viewModel.SelectedCategoryId = null;
        _viewModel.MinPrice = 0;
        _viewModel.MaxPrice = 0;
        _viewModel.UseFuzzySearch = true;
        _viewModel.FuzzyThreshold = 70;

        if (_categoryComboBox != null && _categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }

        if (_fuzzySearchCheckBox != null)
        {
            _fuzzySearchCheckBox.IsChecked = true;
        }

        if (_fuzzyThresholdSlider != null)
        {
            _fuzzyThresholdSlider.Value = 70;
        }

        _plugin.ClearFilter();
        System.Diagnostics.Debug.WriteLine("✅ Filter cleared");
    }

    private async void ShowErrorDialog(string message)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine($"Cannot show dialog: {message}");
        }
    }
}

internal class PluginUIViewModel
{
    private readonly ISearchPlugin _plugin;

    public string Keyword { get; set; } = string.Empty;
    public int? SelectedCategoryId { get; set; }
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; }
    public bool UseFuzzySearch { get; set; } = true;
    public int FuzzyThreshold { get; set; } = 70;

    public PluginUIViewModel(ISearchPlugin plugin)
    {
        _plugin = plugin;
        
        var currentFilter = _plugin.GetCurrentFilter();
        Keyword = currentFilter.Keyword ?? string.Empty;
        SelectedCategoryId = currentFilter.CategoryId;
        MinPrice = currentFilter.MinPrice.HasValue ? (double)currentFilter.MinPrice.Value : 0;
        MaxPrice = currentFilter.MaxPrice.HasValue ? (double)currentFilter.MaxPrice.Value : 0;
        UseFuzzySearch = currentFilter.UseFuzzySearch;
        FuzzyThreshold = currentFilter.FuzzyThreshold;
    }
}
