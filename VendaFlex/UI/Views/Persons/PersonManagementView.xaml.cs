using System.Windows.Controls;
using VendaFlex.ViewModels.Persons;

namespace VendaFlex.UI.Views.Persons
{
    /// <summary>
    /// Interaction logic for PersonManagementView.xaml
    /// </summary>
    public partial class PersonManagementView : Page
    {
        public PersonManagementView(PersonManagementViewModel viewModel)
        {
            InitializeComponent();
           // DataContext = viewModel;
        }
    }
}
