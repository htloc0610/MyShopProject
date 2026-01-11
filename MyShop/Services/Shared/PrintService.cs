using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Printing;

namespace MyShop.Services.Shared;

/// <summary>
/// Service for handling printing operations using Windows Print Manager.
/// Wraps WinUI 3 printing APIs to provide a simple interface for printing UIElements.
/// </summary>
public class PrintService : IPrintService
{
    private PrintDocument? _printDocument;
    private IPrintDocumentSource? _printDocumentSource;
    private PrintManager? _printManager;
    private UIElement? _contentToPrint;
    private List<UIElement> _printPages = new();
    private TaskCompletionSource<bool>? _printTaskCompletionSource;

    /// <summary>
    /// Print a UIElement to the Windows Print Manager.
    /// </summary>
    public async Task<bool> PrintAsync(string documentName, UIElement contentToPrint)
    {
        if (contentToPrint == null)
            throw new ArgumentNullException(nameof(contentToPrint));

        // Clean up any previous print session
        CleanupPrintResources();

        _contentToPrint = contentToPrint;
        _printPages.Clear();
        _printTaskCompletionSource = new TaskCompletionSource<bool>();

        try
        {
            // Get the PrintManager for the current window
            if (App.MainWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("Print error: MainWindow is null");
                throw new InvalidOperationException("Main window is not available for printing");
            }

            // CRITICAL: Ensure the content is measured and arranged before printing
            // This is required for WinUI 3 to properly render the UIElement
            var pageSize = new Windows.Foundation.Size(816, 1056); // A4 size at 96 DPI
            contentToPrint.Measure(pageSize);
            contentToPrint.Arrange(new Windows.Foundation.Rect(0, 0, pageSize.Width, pageSize.Height));
            
            // Force layout update
            contentToPrint.UpdateLayout();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            _printManager = PrintManagerInterop.GetForWindow(hwnd);

            // Register for print task requested event
            _printManager.PrintTaskRequested += OnPrintTaskRequested;

            // Create print document
            _printDocument = new PrintDocument();
            _printDocumentSource = _printDocument.DocumentSource;

            // Register for print document events
            _printDocument.Paginate += OnPaginate;
            _printDocument.GetPreviewPage += OnGetPreviewPage;
            _printDocument.AddPages += OnAddPages;

            // Show the print UI (this is async and returns when dialog is shown, not completed)
            await PrintManagerInterop.ShowPrintUIForWindowAsync(hwnd);

            // Wait for the print task to complete before cleaning up
            // This prevents the "Loading preview" issue on subsequent prints
            await Task.Delay(500); // Small delay to ensure print dialog has processed

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Clean up on error
            CleanupPrintResources();
            
            throw; // Re-throw to let ViewModel handle it
        }
    }

    /// <summary>
    /// Clean up print resources and event handlers.
    /// </summary>
    private void CleanupPrintResources()
    {
        // Unregister print document events
        if (_printDocument != null)
        {
            _printDocument.Paginate -= OnPaginate;
            _printDocument.GetPreviewPage -= OnGetPreviewPage;
            _printDocument.AddPages -= OnAddPages;
        }

        // Unregister print manager events
        if (_printManager != null)
        {
            _printManager.PrintTaskRequested -= OnPrintTaskRequested;
        }

        _printDocument = null;
        _printDocumentSource = null;
        _printManager = null;
        _contentToPrint = null;
        _printPages.Clear();
    }

    /// <summary>
    /// Event handler for print task requested.
    /// Creates the print task with the document name.
    /// </summary>
    private void OnPrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
    {
        var printTask = args.Request.CreatePrintTask("Invoice", sourceRequested =>
        {
            if (_printDocumentSource != null)
            {
                sourceRequested.SetSource(_printDocumentSource);
            }
        });

        // Handle print task completion to clean up resources
        printTask.Completed += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"Print task completed: {e.Completion}");
            
            // Clean up resources after print task completes
            // Use dispatcher to ensure cleanup happens on UI thread
            App.MainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                CleanupPrintResources();
                _printTaskCompletionSource?.TrySetResult(true);
            });
        };
    }

    /// <summary>
    /// Event handler for pagination.
    /// Determines how many pages to print.
    /// </summary>
    private void OnPaginate(object sender, PaginateEventArgs e)
    {
        _printPages.Clear();

        if (_contentToPrint != null)
        {
            // For invoice, we typically have a single page
            _printPages.Add(_contentToPrint);
        }

        // Set the page count
        if (_printDocument != null)
        {
            _printDocument.SetPreviewPageCount(_printPages.Count, PreviewPageCountType.Final);
        }
    }

    /// <summary>
    /// Event handler for getting preview page.
    /// Renders the preview in the print dialog.
    /// </summary>
    private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
    {
        if (_printDocument != null && e.PageNumber > 0 && e.PageNumber <= _printPages.Count)
        {
            _printDocument.SetPreviewPage(e.PageNumber, _printPages[e.PageNumber - 1]);
        }
    }

    /// <summary>
    /// Event handler for adding pages.
    /// Sends the actual pages to the printer.
    /// </summary>
    private void OnAddPages(object sender, AddPagesEventArgs e)
    {
        if (_printDocument != null)
        {
            foreach (var page in _printPages)
            {
                _printDocument.AddPage(page);
            }

            _printDocument.AddPagesComplete();
        }
    }
}
