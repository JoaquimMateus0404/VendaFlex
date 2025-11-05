using System.Windows.Controls;
using VendaFlex.ViewModels.Products;

namespace VendaFlex.UI.Views.Products
{
    /// <summary>
    /// Interaction logic for ProductManagementView.xaml
    /// </summary>
    public partial class ProductManagementView : Page
    {
        public ProductManagementView(ProductManagementViewModel viewModel)
        {
            InitializeComponent();
            //DataContext = viewModel;
        }
    }
}
