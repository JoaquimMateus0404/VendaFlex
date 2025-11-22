using System.Windows;
using System.Windows.Controls;
using VendaFlex.ViewModels.Settings;

namespace VendaFlex.UI.Views.Settings
{
    /// <summary>
    /// Interaction logic for UserProfileView.xaml
    /// </summary>
    public partial class UserProfileView : Page
    {
        private readonly UserProfileViewModel _viewModel;

        public UserProfileView(UserProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            //DataContext = _viewModel;
        }

        // Eventos para sincronizar PasswordBox com ViewModel
        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.CurrentPassword = passwordBox.Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.NewPassword = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
