using System;
using Microsoft.UI.Xaml;

namespace MyShop.Contracts;

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public interface IPaymentPlugin
{
    string Name { get; }
    string Description { get; }
    string IconGlyph { get; } // Icon font glyph
    
    // Initialize the payment session (e.g. start server)
    void Initialize(decimal amount);
    
    // Clean up resources (e.g. stop server)
    void Cleanup();
    
    // Returns the view to display in the main app
    UIElement GetPaymentView();
    
    // Event to notify the app when payment is finished
    event EventHandler<PaymentResult> OnPaymentCompleted;
}
