using System;
using System.Threading.Tasks;

namespace MyShop.Services;

/// <summary>
/// Implementation of IDataService.
/// This service handles data operations for the application.
/// 
/// DI Lifecycle Note:
/// - Registered as SINGLETON: Same instance shared across all requests.
///   Use for stateless services or when you need to maintain state.
/// - Could be TRANSIENT if a new instance is needed each time.
/// </summary>
public class DataService : IDataService
{
    /// <inheritdoc />
    public string GetWelcomeMessage()
    {
        return "Welcome to MyShop! This message comes from the DataService.";
    }

    /// <inheritdoc />
    public async Task<string> LoadDataAsync()
    {
        // Simulate async data loading (e.g., from API or database)
        await Task.Delay(500);
        return $"Data loaded successfully at {DateTime.Now:HH:mm:ss}";
    }
}
