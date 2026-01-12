using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MomoPayment;

public class LocalWebServer
{
    private HttpListener? _listener;
    private bool _isRunning;
    private readonly int _port;

    public event EventHandler? OnPaymentSuccess;
    public event EventHandler? OnPaymentFailed;

    public LocalWebServer(int port = 8888)
    {
        _port = port;
    }

    public void Start()
    {
        if (_isRunning) return;

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{_port}/");
            _listener.Start();
            _isRunning = true;
            
            Task.Run(ListenLoop);
            System.Diagnostics.Debug.WriteLine($"Web Server started on port {_port}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start web server: {ex.Message}");
        }
    }

    public void Stop()
    {
        _isRunning = false;
        try
        {
            _listener?.Stop();
            _listener?.Close();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, ignore
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping web server: {ex.Message}");
        }
        _listener = null;
    }

    private async Task ListenLoop()
    {
        while (_isRunning && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
            catch (HttpListenerException)
            {
                // Listener stopped
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in listener loop: {ex.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        string responseString = "";

        // Handle CORS
        response.AddHeader("Access-Control-Allow-Origin", "*");

        if (request.Url?.AbsolutePath == "/pay")
        {
             // Serve the Payment Page
             string amount = request.QueryString["amt"] ?? "0";
             responseString = GetHtmlPage(amount);
             response.ContentType = "text/html";
        }
        else if (request.Url?.AbsolutePath == "/success")
        {
            // Handle Success Button Click
            OnPaymentSuccess?.Invoke(this, EventArgs.Empty);
            responseString = "<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body><h1 style='color:green;text-align:center;margin-top:50px;'>Payment Successful! You can close this tab.</h1></body></html>";
            response.ContentType = "text/html";
        }
        else if (request.Url?.AbsolutePath == "/fail")
        {
            // Handle Fail Button Click
            OnPaymentFailed?.Invoke(this, EventArgs.Empty);
            responseString = "<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body><h1 style='color:red;text-align:center;margin-top:50px;'>Payment Cancelled.</h1></body></html>";
            response.ContentType = "text/html";
        }
        else
        {
            response.StatusCode = 404;
            responseString = "Not Found";
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private string GetHtmlPage(string amount)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, sans-serif; text-align: center; padding: 20px; background-color: #f5f5f5; }}
        .card {{ background: white; padding: 30px 20px; border-radius: 12px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); max-width: 350px; margin: 0 auto; }}
        h1 {{ color: #A50064; font-size: 24px; margin-bottom: 10px; }}
        .subtitle {{ color: #666; font-size: 14px; }}
        .amount {{ font-size: 28px; font-weight: bold; margin: 25px 0; color: #333; }}
        button {{ width: 100%; padding: 16px; margin: 8px 0; border: none; border-radius: 10px; font-size: 16px; font-weight: 600; cursor: pointer; }}
        .btn-pay {{ background-color: #A50064; color: white; }}
        .btn-cancel {{ background-color: #e8e8e8; color: #555; }}
    </style>
</head>
<body>
    <div class='card'>
        <img src='https://upload.wikimedia.org/wikipedia/vi/f/fe/MoMo_Logo.png' width='60' />
        <h1>Momo Payment</h1>
        <p class='subtitle'>Order from MyShop</p>
        <div class='amount'>{double.Parse(amount):N0} VND</div>
        <form action='/success' method='post'>
            <button class='btn-pay' type='submit'>Confirm Payment</button>
        </form>
        <form action='/fail' method='post'>
            <button class='btn-cancel' type='submit'>Cancel</button>
        </form>
    </div>
</body>
</html>";
    }
}
