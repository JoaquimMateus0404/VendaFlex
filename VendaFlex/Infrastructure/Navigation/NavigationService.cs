using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VendaFlex.UI.Views.Authentication;
using VendaFlex.UI.Views.Dashboard;
using VendaFlex.UI.Views.Setup;
using VendaFlex.ViewModels.Authentication;
using VendaFlex.ViewModels.Dashboard;
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
                NavigateToPage < LoginView, LoginViewModel>("VendaFlex - Login", 1000, 700, closeCurrent: true);
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
                NavigateToPage<InitialSetupView, InitialSetupViewModel>("VendaFlex - Configuração Inicial", 1000, 700, closeCurrent: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Setup");
                throw;
            }
        }

        public void NavigateToDashBoard()
        {
            _logger.LogInformation("Navegando para tela Principal");

            try
            {
                NavigateToPage<DashboardView, DashboardViewModel>("VendaFlex - DashBoard", 1000, 700, closeCurrent: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para DashBoard");
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
                    _logger.LogWarning("MainWindow não encontrada. Abortando navegação para página.");
                    return;
                }

                // Resolver Page e ViewModel do container
                var page = _serviceProvider.GetService(typeof(TView)) as System.Windows.Controls.Page;
                if (page == null)
                {
                    _logger.LogError("Nenhum serviço registrado para a View {ViewType}", typeof(TView).FullName);
                    throw new InvalidOperationException($"No service registered for view {typeof(TView).FullName}");
                }

                var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as object;
                if (viewModel == null)
                {
                    _logger.LogError("Nenhum serviço registrado para o ViewModel {ViewModelType}", typeof(TViewModel).FullName);
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
                _logger.LogInformation("Navegação concluída. Janela atual: {WindowType}", navWindow.GetType().Name);
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
