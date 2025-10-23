using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                        "LoginView ainda não implementada.\n\nEsta funcionalidade será adicionada em breve.",
                        "Em Desenvolvimento",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });
                
                _logger.LogWarning("LoginView ainda não implementada");
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
                // TODO: Criar SetupView e SetupViewModel
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        "SetupView ainda não implementada.\n\nEsta funcionalidade será adicionada em breve.",
                        "Em Desenvolvimento",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });
                
                _logger.LogWarning("SetupView ainda não implementada");
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
                        "MainView ainda não implementada.\n\nEsta funcionalidade será adicionada em breve.",
                        "Em Desenvolvimento",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });
                
                _logger.LogWarning("MainView ainda não implementada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Main");
                throw;
            }
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
