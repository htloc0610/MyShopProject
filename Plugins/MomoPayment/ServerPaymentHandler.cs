using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MomoPayment;

/// <summary>
/// Handles communication with the hosted MoMo payment server.
/// Replaces LocalWebServer for production use.
/// </summary>
public class ServerPaymentHandler : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private CancellationTokenSource? _pollingCts;
    private string? _currentSessionId;

    public event EventHandler? OnPaymentSuccess;
    public event EventHandler? OnPaymentFailed;

    public ServerPaymentHandler(string serverUrl = "https://windowhosting-q4e2.vercel.app")
    {
        _serverUrl = serverUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyShop-MomoPayment/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Creates a payment session on the server and returns the payment URL.
    /// </summary>
    public async Task<(string sessionId, string payUrl)?> CreateSessionAsync(decimal amount)
    {
        try
        {
            var jsonBody = JsonSerializer.Serialize(new { amount = (int)amount });
            System.Diagnostics.Debug.WriteLine($"[MoMo] Creating session: URL={_serverUrl}/api/session, Amount={amount}");
            
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_serverUrl}/api/session", content);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[MoMo] Response: Status={response.StatusCode}, Body={responseBody}");
            
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[MoMo] Failed to create session: {response.StatusCode} - {responseBody}");
                return null;
            }

            using var doc = JsonDocument.Parse(responseBody);
            
            var sessionId = doc.RootElement.GetProperty("sessionId").GetString();
            var payUrl = doc.RootElement.GetProperty("payUrl").GetString();

            System.Diagnostics.Debug.WriteLine($"[MoMo] Session created: ID={sessionId}, PayUrl={payUrl}");

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(payUrl))
            {
                return null;
            }

            _currentSessionId = sessionId;
            return (sessionId, payUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MoMo] Error creating session: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Starts polling the server for payment status.
    /// </summary>
    public void StartPolling()
    {
        if (string.IsNullOrEmpty(_currentSessionId)) return;

        _pollingCts = new CancellationTokenSource();
        _ = PollStatusAsync(_pollingCts.Token);
    }

    /// <summary>
    /// Stops polling for payment status.
    /// </summary>
    public void StopPolling()
    {
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;
    }

    private async Task PollStatusAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1500, ct); // Poll every 1.5 seconds

                var response = await _httpClient.GetAsync($"{_serverUrl}/api/status/{_currentSessionId}", ct);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var status = doc.RootElement.GetProperty("status").GetString();

                    if (status == "success")
                    {
                        OnPaymentSuccess?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    else if (status == "cancelled")
                    {
                        OnPaymentFailed?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    // status == "pending" → continue polling
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        StopPolling();
        _httpClient.Dispose();
    }
}
