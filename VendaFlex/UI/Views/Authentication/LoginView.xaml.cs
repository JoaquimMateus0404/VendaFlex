using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VendaFlex.ViewModels.Authentication;

namespace VendaFlex.UI.Views.Authentication
{
    /// <summary>
    /// Interação lógica para LoginView.xaml
    /// </summary>
    public partial class LoginView : Page
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Focar no campo de usuário ao carregar
            UsernameTextBox.Focus();

            // Carregar credenciais salvas se disponível
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.LoadSavedCredentials();
            }
        }

        // Handle KeyDown at Page level (wired in XAML)
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel viewModel)
            {
                if (viewModel.LoginCommand.CanExecute(null))
                {
                    viewModel.LoginCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel viewModel)
            {
                if (viewModel.LoginCommand.CanExecute(null))
                {
                    viewModel.LoginCommand.Execute(null);
                }
            }
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel viewModel)
            {
                if (viewModel.LoginCommand.CanExecute(null))
                {
                    viewModel.LoginCommand.Execute(null);
                }
            }
        }
    }
}
