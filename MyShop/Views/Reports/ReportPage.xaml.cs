using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels.Reports;
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
            Loaded += (_, _) =>
            {
                ViewModel.LoadReportCommand.Execute(null);
            };
        }
    }
}
