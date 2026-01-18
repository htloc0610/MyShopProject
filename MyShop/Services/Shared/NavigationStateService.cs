using System;
using System.IO;

namespace MyShop.Services.Shared;

/// <summary>
/// Service for managing navigation state persistence using file storage.
/// Remembers the last visited page across app sessions.
/// Uses file-based storage to work with both packaged and unpackaged apps.
/// </summary>
public class NavigationStateService : INavigationStateService
{
    private const string StateFileName = "NavigationState.txt";
    private readonly string _stateFilePath;

    public NavigationStateService()
    {
        // Use LocalApplicationData folder - works for both packaged and unpackaged apps
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyShop");

        // Ensure directory exists
        Directory.CreateDirectory(appDataFolder);

        _stateFilePath = Path.Combine(appDataFolder, StateFileName);
    }

    /// <summary>
    /// Save the last visited page tag to file.
    /// </summary>
    public void SaveLastVisitedPage(string pageTag)
    {
        if (string.IsNullOrWhiteSpace(pageTag))
        {
            return; // Don't save empty/null tags
        }

        try
        {
            File.WriteAllText(_stateFilePath, pageTag);
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Saved page: {pageTag}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Error saving page: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the last visited page tag from file.
    /// </summary>
    /// <returns>The page tag, or null if not found</returns>
    public string? GetLastVisitedPage()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var pageTag = File.ReadAllText(_stateFilePath)?.Trim();
                if (!string.IsNullOrWhiteSpace(pageTag))
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationState] Retrieved page: {pageTag}");
                    return pageTag;
                }
            }

            System.Diagnostics.Debug.WriteLine("[NavigationState] No saved page found (first run)");
            return null;
        }
        catch (Exception ex)
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
            if (File.Exists(_stateFilePath))
            {
                File.Delete(_stateFilePath);
                System.Diagnostics.Debug.WriteLine("[NavigationState] Cleared saved page");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationState] Error clearing page: {ex.Message}");
        }
    }
}
