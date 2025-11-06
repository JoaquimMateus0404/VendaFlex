using System.Windows.Controls;
using VendaFlex.ViewModels.Stock;
using MaterialDesignThemes.Wpf;

namespace VendaFlex.UI.Views.Stock
{
    /// <summary>
    /// Interaction logic for StockManagementView.xaml
    /// </summary>
    public partial class StockManagementView : Page
    {
        public StockManagementView()
        {
            InitializeComponent();
            
            // Conectar o Snackbar ao ViewModel quando o DataContext mudar
            DataContextChanged += (s, e) =>
            {
                if (DataContext is StockManagementViewModel viewModel)
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.StatusMessage) && 
                            !string.IsNullOrEmpty(viewModel.StatusMessage))
                        {
                            StatusSnackbar.MessageQueue?.Enqueue(viewModel.StatusMessage);
                        }
                    };
                }
            };
        }
    }
}
