using Windows.Storage;

namespace MyShop.Services.Shared;

/// <summary>
/// Service for managing navigation state persistence using Windows LocalSettings.
/// Remembers the last visited page across app sessions.
/// </summary>
public class NavigationStateService : INavigationStateService
{
    private const string LastVisitedPageKey = "LastVisitedPage";
    private readonly ApplicationDataContainer _localSettings;

    public NavigationStateService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
    }

    /// <summary>
    /// Save the last visited page tag to LocalSettings.
    /// </summary>
    public void SaveLastVisitedPage(string pageTag)
    {
        if (string.IsNullOrWhiteSpace(pageTag))
        {
            return; // Don't save empty/null tags
        }

        try
        {
            _localSettings.Values[LastVisitedPageKey] = pageTag;
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Saved page: {pageTag}");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Error saving page: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the last visited page tag from LocalSettings.
    /// </summary>
    /// <returns>The page tag, or null if not found</returns>
    public string? GetLastVisitedPage()
    {
        try
        {
            if (_localSettings.Values.TryGetValue(LastVisitedPageKey, out var value))
            {
                var pageTag = value?.ToString();
                System.Diagnostics.Debug.WriteLine($"[NavigationState] Retrieved page: {pageTag}");
                return pageTag;
            }

            System.Diagnostics.Debug.WriteLine("[NavigationState] No saved page found (first run)");
            return null;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Error retrieving page: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Clear the saved navigation state.
    /// </summary>
    public void ClearLastVisitedPage()
    {
        try
        {
            if (_localSettings.Values.ContainsKey(LastVisitedPageKey))
            {
                _localSettings.Values.Remove(LastVisitedPageKey);
                System.Diagnostics.Debug.WriteLine("[NavigationState] Cleared saved page");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Error clearing page: {ex.Message}");
        }
    }
}
