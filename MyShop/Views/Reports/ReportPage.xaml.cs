using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Reports;
using System.ComponentModel;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Views.Reports
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReportPage : Page
    {
        public ReportViewModel ViewModel { get; }
        public ReportPage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ReportViewModel>();
            DataContext = ViewModel;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            Loaded += (_, _) =>
            {
                ViewModel.LoadReportCommand.Execute(null);
            };
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsLoading) && !ViewModel.IsLoading)
            {
                RefreshChart(ProductSalesChart);
                RefreshChart(RevenueProfitChart);
            }
        }

        private void RefreshChart(Control? chart)
        {
            if (chart == null)
                return;

            DispatcherQueue.TryEnqueue(() =>
            {
                chart.Visibility = Visibility.Collapsed;
                chart.Visibility = Visibility.Visible;
                chart.InvalidateMeasure();
                chart.InvalidateArrange();
                chart.UpdateLayout();
                InvokeChartUpdate(chart);
            });
        }

        private static void InvokeChartUpdate(Control chart)
        {
            var update = chart.GetType().GetMethod("Update");
            update?.Invoke(chart, null);
        }
    }
}
