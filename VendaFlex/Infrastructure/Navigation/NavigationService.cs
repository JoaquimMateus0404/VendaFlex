using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VendaFlex.UI.Views.Authentication;
using VendaFlex.UI.Views.Dashboard;
using VendaFlex.UI.Views.Sales;
using VendaFlex.UI.Views.Setup;
using VendaFlex.UI.Views.Settings;
using VendaFlex.ViewModels.Authentication;
using VendaFlex.ViewModels.Dashboard;
using VendaFlex.ViewModels.Sales;
using VendaFlex.ViewModels.Setup;
using VendaFlex.ViewModels.Settings;

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

        #region  View Navigations
        public void NavigateToLogin()
        {
            _logger.LogInformation("Navegando para tela de Login");

            try
            {
                NavigateToPage<LoginView, LoginViewModel>(
                    "VendaFlex - Login",
                    new NavigationOptions
                    {
                        // Login deve substituir a tela anterior e ser a MainWindow
                        Mode = NavigationMode.Replace,
                        SetAsMainWindow = true,

                        // Aparência padrão mais limpa para Login
                        Title = "VendaFlex - Login",
                        Width = 1000,
                        Height = 700,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        WindowState = WindowState.Normal,
                        StartupLocation = WindowStartupLocation.CenterScreen,
                        ShowInTaskbar = true,
                        Topmost = false
                    }
                );
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
                NavigateToPage<InitialSetupView, InitialSetupViewModel>(
                    "VendaFlex - Configuração Inicial",
                    new NavigationOptions
                    {
                        // Setup como diálogo modal para impedir interação com a chamadora
                        Mode = NavigationMode.Replace,
                        Title = "VendaFlex - Configuração Inicial",
                        Width = 1000,
                        Height = 700,
                        WindowStyle = WindowStyle.SingleBorderWindow,
                        ResizeMode = ResizeMode.CanResize,
                        WindowState = WindowState.Normal,
                        StartupLocation = WindowStartupLocation.CenterOwner,
                        ShowInTaskbar = false,
                        Topmost = false
                    }
                );
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
                NavigateToPage<DashboardView, DashboardViewModel>(
                    "VendaFlex - DashBoard",
                    new NavigationOptions
                    {
                        // Dashboard substitui a anterior e vira a MainWindow
                        Mode = NavigationMode.Replace,
                        SetAsMainWindow = true,
                        Title = "VendaFlex - DashBoard",
                        Width = 1200,
                        Height = 800,
                        WindowStyle = WindowStyle.SingleBorderWindow,
                        ResizeMode = ResizeMode.CanResize,
                        WindowState = WindowState.Maximized,
                        StartupLocation = WindowStartupLocation.CenterScreen,
                        ShowInTaskbar = true,
                        Topmost = false
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para DashBoard");
                throw;
            }
        }

        public void NavigateToPdv()
        {
            //implementar agora
            _logger.LogInformation("Navegando para tela de Ponto de Venda");
            try
            {
                NavigateToPage<PdvView, PdvViewModel>(
                    "VendaFlex - Ponto de Venda",
                    new NavigationOptions
                    {
                        Mode = NavigationMode.Stack,
                        Width = 1200,
                        Height = 800,
                        WindowState = WindowState.Maximized 
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para PDV");
                throw;
            }

        }

        public void NavigateToCompanyConfig()
        {
            _logger.LogInformation("Navegando para tela de Configurações da Empresa");
            try
            {
                NavigateToPage<CompanyConfigView, CompanyConfigViewModel>(
                    "VendaFlex - Configurações da Empresa",
                    new NavigationOptions
                    {
                        Mode = NavigationMode.Stack,
                        Width = 1200,
                        Height = 800,
                        WindowStyle = WindowStyle.SingleBorderWindow,
                        ResizeMode = ResizeMode.CanResize,
                        WindowState = WindowState.Maximized,
                        StartupLocation = WindowStartupLocation.CenterScreen,
                        ShowInTaskbar = true,
                        Topmost = false
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para Configurações da Empresa");
                throw;
            }
        }
        #endregion
        public void NavigateToPage<TView, TViewModel>(string title, double width = 1000, double height = 700, bool closeCurrent = false)
            where TView : System.Windows.Controls.Page
            where TViewModel : class
        {
            // Redireciona para a nova API baseada em opções mantendo compatibilidade
            var options = new NavigationOptions
            {
                Title = title,
                Width = width,
                Height = height,
                Mode = closeCurrent ? NavigationMode.Replace : NavigationMode.Stack,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.CanResize,
                WindowState = WindowState.Normal,
                StartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                Topmost = false,
                SetAsMainWindow = closeCurrent
            };

            NavigateToPage<TView, TViewModel>(title, options);
        }

        public void NavigateToPage<TView, TViewModel>(string title, NavigationOptions options)
            where TView : System.Windows.Controls.Page
            where TViewModel : class
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var previous = Application.Current.MainWindow;

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

                // Se for para focar existente, tentar localizar
                if (options.Mode == NavigationMode.FocusExisting)
                {
                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w.Content?.GetType() == typeof(TView))
                        {
                            _logger.LogInformation("Focando janela existente de {View}", typeof(TView).Name);
                            w.Show();
                            w.Activate();
                            w.Focus();
                            _currentWindow = w;
                            return;
                        }
                    }

                    // Não achou existente: cair para Stack por padrão
                    options.Mode = NavigationMode.Stack;
                }

                page.DataContext = viewModel;

                var navWindow = new Window
                {
                    Title = options.Title ?? title,
                    Content = page,
                    Width = options.Width,
                    Height = options.Height,
                    WindowStartupLocation = options.StartupLocation,
                    ShowInTaskbar = options.ShowInTaskbar ?? true,
                    WindowStyle = options.WindowStyle ?? WindowStyle.SingleBorderWindow,
                    ResizeMode = options.ResizeMode ?? ResizeMode.CanResize,
                    Topmost = options.Topmost ?? false,
                };

                if (options.WindowState.HasValue)
                {
                    navWindow.WindowState = options.WindowState.Value;
                }

                // Comportamento por modo
                if (options.Mode == NavigationMode.Dialog)
                {
                    if (previous != null)
                    {
                        navWindow.Owner = previous;
                        navWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    }

                    _logger.LogInformation("Abrindo janela como diálogo: {Title}", navWindow.Title);
                    navWindow.ShowInTaskbar = false; // diálogos normalmente não aparecem na taskbar
                    _currentWindow = navWindow;
                    navWindow.ShowDialog();
                    return;
                }

                // Replace: define como MainWindow e fecha a anterior
                if (options.Mode == NavigationMode.Replace)
                {
                    Application.Current.MainWindow = navWindow;
                    try
                    {
                        previous?.Close();
                    }
                    catch (Exception closeEx)
                    {
                        _logger.LogWarning(closeEx, "Falha ao fechar janela anterior");
                    }
                }
                else
                {
                    // Stack: manter anterior aberta, não alterar MainWindow (a menos que explicitamente pedido)
                    if (options.SetAsMainWindow)
                    {
                        Application.Current.MainWindow = navWindow;
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
