using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace MyShop.Services.Shared;

/// <summary>
/// Interface for printing services.
/// Provides abstraction for WinUI 3 printing functionality.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Print a UIElement to the Windows Print Manager.
    /// Opens the system print dialog for the user to select printer or save as PDF/XPS.
    /// </summary>
    /// <param name="documentName">Name of the document (e.g., "Order_#123")</param>
    /// <param name="contentToPrint">The UIElement to print (e.g., InvoicePrintView)</param>
    /// <returns>True if print was successful or initiated, false if cancelled or failed</returns>
    Task<bool> PrintAsync(string documentName, UIElement contentToPrint);
}
