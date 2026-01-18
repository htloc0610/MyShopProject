using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI;
using Windows.UI;

namespace MomoPayment;

public class PaymentControl : UserControl
{
    private readonly decimal _amount;
    private string? _paymentUrl;
    private Image? _qrImage;
    private TextBlock? _serverText;
    private TextBlock? _statusText;
    private ProgressRing? _loadingRing;
    private Border? _qrBorder;

    private readonly Color _momoColor = Color.FromArgb(255, 163, 0, 101); // #A30065

    public event EventHandler? SimulationRequested;
    public event EventHandler? DialogCloseRequested;

    public PaymentControl(decimal amount)
    {
        _amount = amount;
        Content = BuildUI();
        // Start with loading state
        ShowLoading(true);
    }

    /// <summary>
    /// Sets the payment URL and displays the QR code.
    /// Called after server session is created.
    /// </summary>
    public void SetPaymentUrl(string paymentUrl)
    {
        _paymentUrl = paymentUrl;
        
        DispatcherQueue.TryEnqueue(() =>
        {
            ShowLoading(false);
            LoadQrCode();
        });
    }

    private void ShowLoading(bool isLoading)
    {
        if (_loadingRing != null) _loadingRing.IsActive = isLoading;
        if (_loadingRing != null) _loadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        if (_qrImage != null) _qrImage.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        if (_statusText != null) _statusText.Text = isLoading ? "Đang kết nối server..." : "Đang chờ thanh toán...";
    }

    private UIElement BuildUI()
    {
        var mainGrid = new Grid
        {
            RowDefinitions = 
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } // Body
            },
            Width = 350,
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12)
        };

        // Header Section
        var header = new Grid
        {
            Padding = new Thickness(20, 15, 20, 15),
            Background = new SolidColorBrush(_momoColor),
            CornerRadius = new CornerRadius(12, 12, 0, 0)
        };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var logo = new Image 
        { 
            Source = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/vi/f/fe/MoMo_Logo.png")), 
            Height = 32,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(logo, 0);

        var headerTitle = new TextBlock
        {
            Text = "Thanh toán an toàn",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };
        Grid.SetColumn(headerTitle, 1);
        
        header.Children.Add(logo);
        header.Children.Add(headerTitle);
        Grid.SetRow(header, 0);
        mainGrid.Children.Add(header);

        // Body Section
        var bodyStack = new StackPanel
        {
            Padding = new Thickness(24),
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Amount Summary
        var amountStack = new StackPanel { Spacing = 4 };
        amountStack.Children.Add(new TextBlock 
        { 
            Text = "SỐ TIỀN CẦN THANH TOÁN", 
            FontSize = 10, 
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.Gray),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        amountStack.Children.Add(new TextBlock 
        { 
            Text = $"{_amount:N0} VNĐ", 
            FontSize = 28, 
            FontWeight = Microsoft.UI.Text.FontWeights.Black,
            Foreground = new SolidColorBrush(_momoColor),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        bodyStack.Children.Add(amountStack);

        // QR Code Container
        _qrBorder = new Border
        {
            Width = 190,
            Height = 190,
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Colors.White),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1.5),
            BorderBrush = new SolidColorBrush(_momoColor),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var qrContent = new Grid();
        
        _qrImage = new Image { Stretch = Stretch.Uniform, Visibility = Visibility.Collapsed };
        qrContent.Children.Add(_qrImage);
        
        _loadingRing = new ProgressRing 
        { 
            IsActive = true, 
            Width = 50, 
            Height = 50,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        qrContent.Children.Add(_loadingRing);
        
        _qrBorder.Child = qrContent;
        bodyStack.Children.Add(_qrBorder);

        // Instruction
        var instructionStack = new StackPanel { Spacing = 8 };
        instructionStack.Children.Add(new TextBlock
        {
            Text = "Sử dụng App MoMo hoặc Camera để quét mã",
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        _statusText = new TextBlock
        {
            Text = "Đang kết nối server...",
            FontSize = 12,
            FontStyle = Windows.UI.Text.FontStyle.Italic,
            Foreground = new SolidColorBrush(Colors.Gray),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        instructionStack.Children.Add(_statusText);
        bodyStack.Children.Add(instructionStack);

        // Server Info
        var debugBorder = new Border
        {
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
            CornerRadius = new CornerRadius(6)
        };
        var debugContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };
        debugContent.Children.Add(new TextBlock { Text = "Server:", FontSize = 11, Foreground = new SolidColorBrush(Colors.DimGray) });
        _serverText = new TextBlock { Text = "Cloud", FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Colors.DimGray) };
        debugContent.Children.Add(_serverText);
        debugBorder.Child = debugContent;
        bodyStack.Children.Add(debugBorder);

        // Simulation Button
        var simBtn = new Button
        {
            Content = "Giả lập đã quét (Dev Mode)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 36,
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 10, 0, 0)
        };
        simBtn.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(Color.FromArgb(255, 255, 180, 50));
        simBtn.Resources["ButtonForegroundPointerOver"] = new SolidColorBrush(Colors.White);
        simBtn.Click += (s, e) => {
            SimulationRequested?.Invoke(this, EventArgs.Empty);
            DispatcherQueue.TryEnqueue(async () => {
                await System.Threading.Tasks.Task.Delay(500);
                DialogCloseRequested?.Invoke(this, EventArgs.Empty);
            });
        };
        bodyStack.Children.Add(simBtn);

        // Info Footer
        bodyStack.Children.Add(new TextBlock
        {
            Text = "ⓘ Quét mã QR bằng điện thoại để thanh toán",
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.Gray),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, -5, 0, 0)
        });

        Grid.SetRow(bodyStack, 1);
        mainGrid.Children.Add(bodyStack);

        return mainGrid;
    }

    private void LoadQrCode()
    {
        if (string.IsNullOrEmpty(_paymentUrl))
        {
            UpdateStatus("Không thể kết nối server!");
            return;
        }

        // Use public API to generate QR Code image for server URL
        string qrApiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(_paymentUrl)}";
        
        if (_qrImage != null)
        {
            _qrImage.Source = new BitmapImage(new Uri(qrApiUrl));
        }
    }
    
    public void UpdateStatus(string message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_statusText != null)
            {
                _statusText.Text = message;
            }
        });
    }

    public void RequestDialogClose()
    {
        DialogCloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
