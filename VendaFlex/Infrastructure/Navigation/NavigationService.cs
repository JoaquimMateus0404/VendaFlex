using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VendaFlex.UI.Views.Setup;
using VendaFlex.ViewModels.Setup;

namespace VendaFlex.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NavigationService> _logger;
        private Window? _currentWindow;

        public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
            _logger.LogInformation("Janela atual definida: {WindowType}", window.GetType().Name);
        }

        public void NavigateToLogin()
        {
            _logger.LogInformation("Navegando para tela de Login");

            try
            {
                // TODO: Criar LoginView e LoginViewModel
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        "LoginView ainda n�o implementada.\n\nEsta funcionalidade ser� adicionada em breve.",
                        "Em Desenvolvimento",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });

                _logger.LogWarning("LoginView ainda n�o implementada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Login");
                throw;
            }
        }

        public void NavigateToSetup()
        {
            _logger.LogInformation("Navegando para tela de Setup");
            try
            {
                NavigateToPage<InitialSetupView, InitialSetupViewModel>("VendaFlex - Configura��o Inicial", 1000, 700, closeCurrent: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Setup");
                throw;
            }
        }

        public void NavigateToMain()
        {
            _logger.LogInformation("Navegando para tela Principal");

            try
            {
                // TODO: Criar MainView e MainViewModel
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        "MainView ainda n�o implementada.\n\nEsta funcionalidade ser� adicionada em breve.",
                        "Em Desenvolvimento",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });

                _logger.LogWarning("MainView ainda n�o implementada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Main");
                throw;
            }
        }

        public void NavigateToPage<TView, TViewModel>(string title, double width = 1000, double height = 700, bool closeCurrent = true)
            where TView : System.Windows.Controls.Page
            where TViewModel : class
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var splash = Application.Current.MainWindow;
                if (splash == null)
                {
                    _logger.LogWarning("MainWindow n�o encontrada. Abortando navega��o para p�gina.");
                    return;
                }

                // Resolver Page e ViewModel do container
                var page = _serviceProvider.GetService(typeof(TView)) as System.Windows.Controls.Page;
                if (page == null)
                {
                    _logger.LogError("Nenhum servi�o registrado para a View {ViewType}", typeof(TView).FullName);
                    throw new InvalidOperationException($"No service registered for view {typeof(TView).FullName}");
                }

                var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as object;
                if (viewModel == null)
                {
                    _logger.LogError("Nenhum servi�o registrado para o ViewModel {ViewModelType}", typeof(TViewModel).FullName);
                    throw new InvalidOperationException($"No service registered for viewmodel {typeof(TViewModel).FullName}");
                }

                page.DataContext = viewModel;

                var navWindow = new Window
                {
                    Title = title,
                    Content = page,
                    Width = width,
                    Height = height,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ShowInTaskbar = true,
                    WindowStyle = WindowStyle.SingleBorderWindow,
                    ResizeMode = ResizeMode.CanResize
                };

                Application.Current.MainWindow = navWindow;

                if (closeCurrent)
                {
                    try
                    {
                        splash.Close();
                    }
                    catch (Exception closeEx)
                    {
                        _logger.LogWarning(closeEx, "Falha ao fechar janela atual");
                    }
                }

                navWindow.Show();
                navWindow.Activate();
                navWindow.Focus();

                _currentWindow = navWindow;
                _logger.LogInformation("Navega��o conclu�da. Janela atual: {WindowType}", navWindow.GetType().Name);
            });
        }

        public void CloseCurrentWindow()
        {
            if (_currentWindow != null)
            {
                _logger.LogInformation("Fechando janela atual: {WindowType}", _currentWindow.GetType().Name);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentWindow.Close();
                    _currentWindow = null;
                });
            }
            else
            {
                _logger.LogWarning("Tentativa de fechar janela, mas nenhuma janela atual definida");
            }
        }
    }
}
