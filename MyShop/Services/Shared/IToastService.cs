using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Dispatching;

namespace MyShop.Services.Shared;

/// <summary>
/// Service for displaying global toast notifications.
/// Thread-safe and can be called from background threads.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Initialize the service with the notification control and dispatcher queue.
    /// Must be called before using any Show methods.
    /// </summary>
    void Initialize(InAppNotification notificationControl, DispatcherQueue dispatcherQueue);

    /// <summary>
    /// Show a success notification (green).
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="durationMs">Duration in milliseconds (default: 3000)</param>
    void ShowSuccess(string message, int durationMs = 3000);

    /// <summary>
    /// Show an error notification (red).
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="durationMs">Duration in milliseconds (default: 5000)</param>
    void ShowError(string message, int durationMs = 5000);

    /// <summary>
    /// Show a warning notification (orange).
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="durationMs">Duration in milliseconds (default: 4000)</param>
    void ShowWarning(string message, int durationMs = 4000);

    /// <summary>
    /// Show an info notification (blue).
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="durationMs">Duration in milliseconds (default: 3000)</param>
    void ShowInfo(string message, int durationMs = 3000);
}
