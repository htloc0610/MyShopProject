using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models.Customers;
using MyShop.Models.Products;
using MyShop.Models.Orders;
using MyShop.Services.Customers;
using MyShop.Services.Products;
using MyShop.Services.Orders;
using System.Diagnostics;

namespace MyShop.ViewModels.Orders;

/// <summary>
/// ViewModel for creating new orders with customer selection and cart management.
/// </summary>
public partial class CreateOrderViewModel : ObservableObject
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;

    #endregion

    #region Observable Properties - Customer Selection

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<Customer> _customerSuggestions = new();

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    #endregion

    #region Observable Properties - Product Selection

    [ObservableProperty]
    private ObservableCollection<Product> _productSuggestions = new();

    [ObservableProperty]
    private string _productSearchText = string.Empty;

    #endregion

    #region Observable Properties - Cart Management

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubTotal))]
    [NotifyPropertyChangedFor(nameof(FinalAmount))]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    private ObservableCollection<CartItem> _cartItems = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FinalAmount))]
    private decimal _discountAmount;

    [ObservableProperty]
    private string _couponCode = string.Empty;

    [ObservableProperty]
    private string _couponMessage = string.Empty;

    [ObservableProperty]
    private bool _isCouponValid;

    [ObservableProperty]
    private ObservableCollection<AvailableCoupon> _availableCoupons = new();

    /// <summary>
    /// Subtotal before discount.
    /// </summary>
    public decimal SubTotal => CartItems.Sum(item => item.Total);

    /// <summary>
    /// Final amount after discount.
    /// </summary>
    public decimal FinalAmount => SubTotal - DiscountAmount;

    /// <summary>
    /// Check if cart has items.
    /// </summary>
    public bool HasItems => CartItems.Count > 0;

    #endregion

    #region Observable Properties - UI State

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    #endregion

    #region Constructor

    public CreateOrderViewModel(ICustomerService customerService, IProductService productService, IOrderService orderService)
    {
        _customerService = customerService;
        _productService = productService;
        _orderService = orderService;

        // Subscribe to cart changes
        CartItems.CollectionChanged += (s, e) =>
        {
            UpdateTotals();
            
            // Subscribe to quantity changes for each item
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += OnCartItemPropertyChanged;
                }
            }
            
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                {
                    item.PropertyChanged -= OnCartItemPropertyChanged;
                }
            }
            
            // Re-apply coupon if one was applied
            if (IsCouponValid && !string.IsNullOrWhiteSpace(CouponCode))
            {
                _ = ReApplyCouponAsync();
            }
        };
    }

    private void OnCartItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartItem.Quantity) || e.PropertyName == nameof(CartItem.Total))
        {
            UpdateTotals();
            
            // Re-apply coupon if one was applied
            if (IsCouponValid && !string.IsNullOrWhiteSpace(CouponCode))
            {
                _ = ReApplyCouponAsync();
            }
        }
    }

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        // Load available coupons
        await LoadAvailableCouponsAsync();
    }

    #endregion

    #region Commands - Customer Search

    /// <summary>
    /// Search for customers based on keyword.
    /// </summary>
    [RelayCommand]
    private async Task SearchCustomersAsync(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
        {
            CustomerSuggestions.Clear();
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            
            var result = await _customerService.GetCustomersAsync(
                page: 1,
                pageSize: 10,
                keyword: searchText
            );

            CustomerSuggestions.Clear();
            foreach (var customer in result.Items)
            {
                CustomerSuggestions.Add(customer);
            }
        }
        catch (Exception)
        {
            CustomerSuggestions.Clear();
            // Don't show error for search, just clear suggestions
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Select a customer from suggestions.
    /// </summary>
    [RelayCommand]
    private void SelectCustomer(Customer customer)
    {
        SelectedCustomer = customer;
        
        CustomerSearchText = customer.Name;
        CustomerSuggestions.Clear();
    }

    /// <summary>
    /// Clear selected customer.
    /// </summary>
    [RelayCommand]
    private void ClearCustomer()
    {
        SelectedCustomer = null;
        CustomerSearchText = string.Empty;
        CustomerSuggestions.Clear();
    }

    #endregion

    #region Commands - Product Search

    /// <summary>
    /// Search for products based on keyword.
    /// </summary>
    [RelayCommand]
    private async Task SearchProductsAsync(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
        {
            ProductSuggestions.Clear();
            return;
        }

        try
        {
            var result = await _productService.GetProductsPagedAsync(
                page: 1,
                pageSize: 10,
                keyword: searchText
            );

            ProductSuggestions.Clear();
            foreach (var product in result.Items)
            {
                // Only show products that are in stock
                if (product.Stock > 0)
                {
                    ProductSuggestions.Add(product);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching products: {ex.Message}");
            ProductSuggestions.Clear();
        }
    }

    #endregion

    #region Commands - Cart Management

    /// <summary>
    /// Add a product to the cart.
    /// </summary>
    [RelayCommand]
    private void AddToCart(Product product)
    {
        if (product == null) return;

        // Check if product already in cart
        var existingItem = CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            CartItems.Add(new CartItem(product, 1));
        }

        // Clear product search
        ProductSearchText = string.Empty;
        ProductSuggestions.Clear();

        UpdateTotals();
    }

    /// <summary>
    /// Remove a product from the cart.
    /// </summary>
    [RelayCommand]
    private void RemoveFromCart(CartItem item)
    {
        if (item == null) return;
        CartItems.Remove(item);
        UpdateTotals();
        
        // Clear coupon if cart is empty
        if (!HasItems)
        {
            ClearCoupon();
        }
    }

    /// <summary>
    /// Clear the entire cart.
    /// </summary>
    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        ClearCoupon();
        UpdateTotals();
    }

    #endregion

    #region Commands - Coupon Management

    /// <summary>
    /// Apply and validate coupon code.
    /// </summary>
    [RelayCommand]
    private async Task ApplyCouponAsync()
    {
        if (string.IsNullOrWhiteSpace(CouponCode))
        {
            CouponMessage = "Vui lòng nhập mã giảm giá";
            IsCouponValid = false;
            return;
        }

        if (!HasItems)
        {
            CouponMessage = "Vui lòng thêm sản phẩm trước khi áp dụng mã giảm giá";
            IsCouponValid = false;
            return;
        }

        try
        {
            IsLoading = true;
            
            // Build preview request
            var request = new OrderPreviewRequest
            {
                Items = CartItems.Select(item => new OrderItemRequest
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity
                }).ToList(),
                CouponCode = CouponCode
            };

            // Call preview API
            var response = await _orderService.PreviewOrderAsync(request);
            
            if (response != null)
            {
                DiscountAmount = response.DiscountAmount;
                CouponMessage = response.CouponMessage ?? "Áp dụng mã thành công";
                IsCouponValid = response.DiscountAmount > 0;
                
                UpdateTotals();
            }
            else
            {
                CouponMessage = "Không thể xác thực mã giảm giá";
                IsCouponValid = false;
                DiscountAmount = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying coupon: {ex.Message}");
            CouponMessage = "Lỗi khi áp dụng mã giảm giá";
            IsCouponValid = false;
            DiscountAmount = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Clear applied coupon.
    /// </summary>
    [RelayCommand]
    private void ClearCoupon()
    {
        CouponCode = string.Empty;
        CouponMessage = string.Empty;
        DiscountAmount = 0;
        IsCouponValid = false;
        UpdateTotals();
    }

    /// <summary>
    /// Re-apply coupon automatically when cart changes.
    /// </summary>
    private async Task ReApplyCouponAsync()
    {
        if (!HasItems)
        {
            ClearCoupon();
            return;
        }

        try
        {
            // Build preview request
            var request = new OrderPreviewRequest
            {
                Items = CartItems.Select(item => new OrderItemRequest
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity
                }).ToList(),
                CouponCode = CouponCode
            };

            // Call preview API silently
            var response = await _orderService.PreviewOrderAsync(request);
            
            if (response != null)
            {
                DiscountAmount = response.DiscountAmount;
                CouponMessage = response.CouponMessage ?? "Áp dụng mã thành công";
                IsCouponValid = response.DiscountAmount > 0;
                
                UpdateTotals();
            }
        }
        catch
        {
            // Silently fail - don't disrupt UX
            // User can re-apply manually if needed
        }
    }

    #endregion

    #region Commands - Order Creation

    /// <summary>
    /// Create the order.
    /// </summary>
    [RelayCommand]
    private async Task CreateOrderAsync()
    {
        if (!HasItems)
        {
            ErrorMessage = "Vui lòng thêm sản phẩm vào đơn hàng";
            HasError = true;
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;

            var request = new OrderCheckoutRequest
            {
                Items = CartItems.Select(item => new OrderItemRequest
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity
                }).ToList(),
                CustomerId = SelectedCustomer?.Id,
                CouponCode = IsCouponValid ? CouponCode : null
            };

            var response = await _orderService.CheckoutOrderAsync(request);
            
            if (response != null)
            {
                // Success! Clear form
                CartItems.Clear();
                SelectedCustomer = null;
                CustomerSearchText = string.Empty;
                ClearCoupon();
                
                ErrorMessage = $"Tạo đơn hàng #{response.OrderId} thành công!";
                HasError = false;
            }
            else
            {
                ErrorMessage = "Không thể tạo đơn hàng. Vui lòng thử lại.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load available coupons from API.
    /// </summary>
    private async Task LoadAvailableCouponsAsync()
    {
        try
        {
            var coupons = await _orderService.GetAvailableCouponsAsync();
            AvailableCoupons.Clear();
            foreach (var coupon in coupons)
            {
                AvailableCoupons.Add(coupon);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading available coupons: {ex.Message}");
        }
    }

    /// <summary>
    /// Select a coupon from available coupons list.
    /// </summary>
    [RelayCommand]
    private async void SelectCoupon(AvailableCoupon coupon)
    {
        if (coupon == null) return;

        // Set coupon code and auto-apply
        CouponCode = coupon.Code;
        await ApplyCouponAsync();
    }

    /// <summary>
    /// Updates totals when cart changes.
    /// </summary>
    private void UpdateTotals()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(FinalAmount));
        OnPropertyChanged(nameof(HasItems));
    }

    #endregion
}
