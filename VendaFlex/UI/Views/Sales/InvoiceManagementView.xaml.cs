using System.Windows.Controls;
using VendaFlex.ViewModels.Sales;

namespace VendaFlex.UI.Views.Sales
{
    /// <summary>
    /// Interaction logic for InvoiceManagementView.xaml
    /// </summary>
    public partial class InvoiceManagementView : Page
    {
        public InvoiceManagementView()
        {
            InitializeComponent();
            
            // Conectar o Snackbar ao ViewModel quando o DataContext mudar
            DataContextChanged += (s, e) =>
            {
                if (DataContext is InvoiceManagementViewModel viewModel)
                {
                    // O MessageQueue já está configurado no ViewModel e vinculado via Binding
                    // Não é necessário fazer nada adicional aqui
                }
            };
        }
    }
}

