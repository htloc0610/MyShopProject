using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls; // Add this for TextBlock
using MyShop.Contracts;

namespace MomoPayment;

public class MomoPaymentPlugin : IPaymentPlugin
{
    private LocalWebServer? _server;
    private PaymentControl? _control;
    private const int PORT = 8888;

    public string Name => "Momo E-Wallet";
    public string Description => "Thanh toán qua ví điện tử Momo (Quét QR)";
    public string IconGlyph => "\uE8C7"; // Wallet Icon

    public event EventHandler<PaymentResult>? OnPaymentCompleted;

    public void Initialize(decimal amount)
    {
        _server = new LocalWebServer(PORT);
        _server.OnPaymentSuccess += Server_OnPaymentSuccess;
        _server.OnPaymentFailed += Server_OnPaymentFailed;
        _server.Start();

        _control = new PaymentControl(amount, PORT);
        _control.SimulationRequested += Server_OnPaymentSuccess;
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
        if (_server != null)
        {
            _server.Stop();
            _server.OnPaymentSuccess -= Server_OnPaymentSuccess;
            _server.OnPaymentFailed -= Server_OnPaymentFailed;
            _server = null;
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
