using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Contracts;

namespace MomoPayment;

public class MomoPaymentPlugin : IPaymentPlugin
{
    private ServerPaymentHandler? _serverHandler;
    private PaymentControl? _control;
    private decimal _amount;

    public string Name => "Momo E-Wallet";
    public string Description => "Thanh toán qua ví điện tử Momo (Quét QR)";
    public string IconGlyph => "\uE8C7"; // Wallet Icon

    public event EventHandler<PaymentResult>? OnPaymentCompleted;

    public void Initialize(decimal amount)
    {
        _amount = amount;
        
        // Create control immediately with loading state
        _control = new PaymentControl(amount);
        _control.SimulationRequested += Server_OnPaymentSuccess;
        
        // Initialize server handler and start async session creation
        _serverHandler = new ServerPaymentHandler();
        _serverHandler.OnPaymentSuccess += Server_OnPaymentSuccess;
        _serverHandler.OnPaymentFailed += Server_OnPaymentFailed;
        
        // Start async initialization (fire and forget, updates control when ready)
        _ = InitializeServerSessionAsync(amount);
    }

    private async System.Threading.Tasks.Task InitializeServerSessionAsync(decimal amount)
    {
        try
        {
            // Create session on server
            var session = await _serverHandler!.CreateSessionAsync(amount);
            
            if (session.HasValue)
            {
                // Update control with server payment URL
                _control?.SetPaymentUrl(session.Value.payUrl);
                
                // Start polling for payment status
                _serverHandler.StartPolling();
            }
            else
            {
                _control?.UpdateStatus("Không thể kết nối server!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Server init error: {ex.Message}");
            _control?.UpdateStatus("Lỗi kết nối server!");
        }
    }

    public UIElement GetPaymentView()
    {
        if (_control != null)
        {
            return _control;
        }
        else
        {
            return new TextBlock { Text = "Plugin not initialized" };
        }
    }

    public void Cleanup()
    {
        if (_serverHandler != null)
        {
            _serverHandler.StopPolling();
            _serverHandler.OnPaymentSuccess -= Server_OnPaymentSuccess;
            _serverHandler.OnPaymentFailed -= Server_OnPaymentFailed;
            _serverHandler.Dispose();
            _serverHandler = null;
        }
    }

    private void Server_OnPaymentSuccess(object? sender, EventArgs e)
    {
        // Must marshal to UI thread if updating UI
        _control?.DispatcherQueue.TryEnqueue(async () => 
        {
            _control.UpdateStatus("Thanh toán thành công!");
            
            // Wait a moment to let user see the status, then close dialog
            await System.Threading.Tasks.Task.Delay(2000);
            _control.RequestDialogClose();
        });

        OnPaymentCompleted?.Invoke(this, new PaymentResult 
        { 
            IsSuccess = true, 
            Message = "Thanh toán đã được xác nhận",
            TransactionId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
        });
    }

    private void Server_OnPaymentFailed(object? sender, EventArgs e)
    {
        _control?.DispatcherQueue.TryEnqueue(() => 
        {
            _control.UpdateStatus("Thanh toán bị hủy! \n Đang chờ thanh toán lại...");
        });

        OnPaymentCompleted?.Invoke(this, new PaymentResult 
        { 
            IsSuccess = false, 
            Message = "Thanh toán bị hủy \n Đang chờ thanh toán lại..." 
        });
    }
}
