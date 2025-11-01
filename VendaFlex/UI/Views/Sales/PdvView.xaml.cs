using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VendaFlex.ViewModels.Sales;

namespace VendaFlex.UI.Views.Sales
{
    /// <summary>
    /// Code-behind para PdvView.xaml
    /// Agora com mínima lógica: foca inputs e delega comandos/estado à ViewModel.
    /// </summary>
    public partial class PdvView : Page
    {
        public PdvView()
        {
            InitializeComponent();

            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Foco na busca rápida
            ProductSearchBox?.Focus();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PdvViewModel vm)
            {
                vm.Cleanup();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (DataContext is not PdvViewModel vm)
                return;

            // F1 - Abrir catálogo
            if (e.Key == Key.F1 && vm.OpenCatalogCommand.CanExecute(null))
            {
                vm.OpenCatalogCommand.Execute(null);
                e.Handled = true;
            }

            // ESC - Fechar catálogo
            if (e.Key == Key.Escape && vm.CloseCatalogCommand.CanExecute(null))
            {
                vm.CloseCatalogCommand.Execute(null);
                e.Handled = true;
            }

            // F9 - Finalizar venda
            if (e.Key == Key.F9 && vm.FinalizeSaleCommand.CanExecute(null))
            {
                vm.FinalizeSaleCommand.Execute(null);
                e.Handled = true;
            }

            // F12 - Limpar carrinho
            if (e.Key == Key.F12 && vm.ClearCartCommand.CanExecute(null))
            {
                vm.ClearCartCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}