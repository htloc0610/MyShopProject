using System.Threading.Tasks;

namespace MyShop.Services;

/// <summary>
/// Defines the contract for data operations.
/// This abstraction allows for easy testing and swapping of implementations.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Gets a welcome message for the application.
    /// </summary>
    string GetWelcomeMessage();

    /// <summary>
    /// Loads data asynchronously (simulated).
    /// </summary>
    Task<string> LoadDataAsync();
}
