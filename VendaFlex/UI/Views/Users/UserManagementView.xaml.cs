using System;
using System.Windows;
using System.Windows.Controls;
using VendaFlex.ViewModels.Users;

namespace VendaFlex.UI.Views.Users
{
    /// <summary>
    /// Interação lógica para UserManagementView.xaml
    /// </summary>
    public partial class UserManagementView : Page
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void EnableEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.IsEditMode = true;
            }
        }

        private void CancelEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.IsEditMode = false;
            }
        }
    }
}
