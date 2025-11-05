using System.Windows.Controls;
using VendaFlex.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VendaFlex.UI.Views.Settings
{
    /// <summary>
    /// Interaction logic for CompanyConfigView.xaml
    /// </summary>
    public partial class CompanyConfigView : Page
    {
        private CompanyConfigViewModel? _viewModel;

        public CompanyConfigView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                try
                {
                    // Obter ViewModel via DI
                    var serviceProvider = ((App)System.Windows.Application.Current).ServiceProvider;
                    _viewModel = serviceProvider.GetRequiredService<CompanyConfigViewModel>();
                    DataContext = _viewModel;

                    // Carregar dados
                    await _viewModel.LoadAsync();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Erro ao inicializar a página: {ex.Message}",
                        "Erro",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}
