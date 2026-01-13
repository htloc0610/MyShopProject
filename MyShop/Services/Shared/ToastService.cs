using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;

namespace MyShop.Services.Shared;

/// <summary>
/// Implementation of toast notification service.
/// Thread-safe using DispatcherQueue for UI thread marshaling.
/// </summary>
public class ToastService : IToastService
{
    private InAppNotification? _notificationControl;
    private DispatcherQueue? _dispatcherQueue;

    /// <summary>
    /// Initialize the service with the notification control and dispatcher queue.
    /// </summary>
    public void Initialize(InAppNotification notificationControl, DispatcherQueue dispatcherQueue)
    {
        _notificationControl = notificationControl;
        _dispatcherQueue = dispatcherQueue;
    }

    /// <summary>
    /// Show a success notification (green background).
    /// </summary>
    public void ShowSuccess(string message, int durationMs = 3000)
    {
        ShowNotification(message, NotificationSeverity.Success, durationMs);
    }

    /// <summary>
    /// Show an error notification (red background).
    /// </summary>
    public void ShowError(string message, int durationMs = 5000)
    {
        ShowNotification(message, NotificationSeverity.Error, durationMs);
    }

    /// <summary>
    /// Show a warning notification (orange background).
    /// </summary>
    public void ShowWarning(string message, int durationMs = 4000)
    {
        ShowNotification(message, NotificationSeverity.Warning, durationMs);
    }

    /// <summary>
    /// Show an info notification (blue background).
    /// </summary>
    public void ShowInfo(string message, int durationMs = 3000)
    {
        ShowNotification(message, NotificationSeverity.Info, durationMs);
    }

    /// <summary>
    /// Internal method to show notification with proper styling and thread safety.
    /// </summary>
    private void ShowNotification(string message, NotificationSeverity severity, int durationMs)
    {
        if (_notificationControl == null || _dispatcherQueue == null)
        {
            System.Diagnostics.Debug.WriteLine("[ToastService] Not initialized. Call Initialize() first.");
            return;
        }

        // Marshal to UI thread using DispatcherQueue
        _dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // Get colors based on severity
                var (backgroundColor, foregroundColor) = GetColors(severity);

                // Apply background and foreground colors
                _notificationControl.Background = new SolidColorBrush(backgroundColor);
                _notificationControl.Foreground = new SolidColorBrush(foregroundColor);

                // Show the notification
                _notificationControl.Show(message, durationMs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ToastService] Error showing notification: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Get color styling based on notification severity.
    /// </summary>
    private (Color backgroundColor, Color foregroundColor) GetColors(NotificationSeverity severity)
    {
        return severity switch
        {
            NotificationSeverity.Success => (Colors.Green, Colors.White),
            NotificationSeverity.Error => (Colors.Red, Colors.White),
            NotificationSeverity.Warning => (Colors.Orange, Colors.White),
            NotificationSeverity.Info => (Colors.DodgerBlue, Colors.White),
            _ => (Colors.Gray, Colors.White)
        };
    }

    /// <summary>
    /// Notification severity levels.
    /// </summary>
    private enum NotificationSeverity
    {
        Success,
        Error,
        Warning,
        Info
    }
}
