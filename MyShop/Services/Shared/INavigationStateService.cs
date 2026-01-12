namespace MyShop.Services.Shared;

/// <summary>
/// Interface for managing navigation state persistence.
/// Saves and restores the last visited page across app sessions.
/// </summary>
public interface INavigationStateService
{
    /// <summary>
    /// Save the last visited page tag to local storage.
    /// </summary>
    /// <param name="pageTag">The tag of the page (e.g., "ProductList", "Dashboard")</param>
    void SaveLastVisitedPage(string pageTag);

    /// <summary>
    /// Get the last visited page tag from local storage.
    /// </summary>
    /// <returns>The page tag, or null if no page has been saved</returns>
    string? GetLastVisitedPage();

    /// <summary>
    /// Clear the saved navigation state.
    /// Optional: Can be called on logout if desired.
    /// </summary>
    void ClearLastVisitedPage();
}
