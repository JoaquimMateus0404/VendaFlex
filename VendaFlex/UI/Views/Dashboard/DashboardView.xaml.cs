using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VendaFlex.ViewModels.Dashboard;
using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Navigation;

namespace VendaFlex.UI.Views.Dashboard
{
    /// <summary>
    /// Code-behind para DashboardView com funcionalidades de popup
    /// ✅ Popups de notificações e perfil
    /// ✅ Hover effects nas linhas da tabela
    /// ✅ Animações suaves
    /// ✅ ViewModel dinâmico com dados do banco
    /// </summary>
    public partial class DashboardView : Page
    {
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private bool _isDataLoaded = false;
        private DispatcherTimer? _notificationTimer;

        public DashboardView(DashboardViewModel viewModel, ISessionService sessionService, INavigationService navigationService)
        {
            InitializeComponent();
            _sessionService = sessionService;
            _navigationService = navigationService;

            // Log para debug
            System.Diagnostics.Debug.WriteLine($"DashboardView: Construtor chamado. ViewModel != null: {viewModel != null}");
            
            // Garantir que o evento Loaded está registrado
            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Evitar carregamento duplicado
            if (!_isDataLoaded)
            {
                _isDataLoaded = true;

                // Obter ViewModel do DataContext (configurado pelo NavigationService)
                if (DataContext is not DashboardViewModel viewModel)
                {
                    System.Diagnostics.Debug.WriteLine("❌ DashboardView: DataContext não é DashboardViewModel!");
                    return;
                }

                // Iniciar animação de entrada
                var storyboard = (Storyboard)this.Resources["CardEntrance"];
                storyboard?.Begin();

                // Carregar dados do dashboard
                System.Diagnostics.Debug.WriteLine("DashboardView: Chamando LoadDashboardDataAsync...");
                await viewModel.LoadDashboardDataAsync();
                System.Diagnostics.Debug.WriteLine("DashboardView: LoadDashboardDataAsync concluído");
                System.Diagnostics.Debug.WriteLine($"DashboardView: Metrics.Count = {viewModel.Metrics?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"DashboardView: TopProducts.Count = {viewModel.TopProducts?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"DashboardView: RecentInvoices.Count = {viewModel.RecentInvoices?.Count ?? 0}");
            }

            // Timer para atualizar notificações em tempo real (a cada 15s)
            if (DataContext is DashboardViewModel vm)
            {
                _notificationTimer ??= new DispatcherTimer
                {
                    Interval = System.TimeSpan.FromSeconds(15)
                };
                _notificationTimer.Tick -= NotificationTimer_Tick;
                _notificationTimer.Tick += NotificationTimer_Tick;
                _notificationTimer.Start();
            }
        }

        private async void NotificationTimer_Tick(object? sender, System.EventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                await vm.RefreshNotificationsAsync();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_notificationTimer != null)
            {
                _notificationTimer.Stop();
                _notificationTimer.Tick -= NotificationTimer_Tick;
                _notificationTimer = null;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _sessionService.EndSession();
                _navigationService.NavigateToLogin();
            }
            catch
            {
                // fallback simples
                Application.Current.Shutdown();
            }
        }

        private void SalesButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToPdv();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToCompanyConfig();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToProductManagement();
        }
        private void StockButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToStockManagement();
        }

        #region Popup Handlers

        /// <summary>
        /// Abre o popup de notificações
        /// </summary>
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            // Fechar popup de perfil se estiver aberto
            ProfilePopup.IsOpen = false;

            // Toggle do popup de notificações
            NotificationPopup.IsOpen = !NotificationPopup.IsOpen;
        }

        /// <summary>
        /// Abre o popup de perfil
        /// </summary>
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Fechar popup de notificações se estiver aberto
            NotificationPopup.IsOpen = false;

            // Toggle do popup de perfil
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        #endregion

        #region Hover Effects

        /// <summary>
        /// Efeito hover nas linhas de faturas
        /// </summary>
        private void InvoiceRow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Animar mudança de cor de fundo
                var colorAnimation = new ColorAnimation
                {
                    To = (Color)ColorConverter.ConvertFromString("#F9FAFB"),
                    Duration = TimeSpan.FromMilliseconds(200)
                };

                var brush = new SolidColorBrush(Colors.Transparent);
                border.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                // Mudar cursor
                border.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Remove efeito hover das linhas de faturas
        /// </summary>
        private void InvoiceRow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Animar volta ao transparente
                var colorAnimation = new ColorAnimation
                {
                    To = Colors.Transparent,
                    Duration = TimeSpan.FromMilliseconds(200)
                };

                if (border.Background is SolidColorBrush brush)
                {
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }

                border.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Efeito hover nos itens de notificação
        /// </summary>
        private void NotificationItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var colorAnimation = new ColorAnimation
                {
                    To = (Color)ColorConverter.ConvertFromString("#F9FAFB"),
                    Duration = TimeSpan.FromMilliseconds(150)
                };

                var brush = new SolidColorBrush(Colors.Transparent);
                border.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                border.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Remove efeito hover dos itens de notificação
        /// </summary>
        private void NotificationItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var colorAnimation = new ColorAnimation
                {
                    To = Colors.Transparent,
                    Duration = TimeSpan.FromMilliseconds(150)
                };

                if (border.Background is SolidColorBrush brush)
                {
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }

                border.Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Sidebar Toggle (Opcional - para implementação futura)

        private bool _sidebarExpanded = true;

        /// <summary>
        /// Alterna entre sidebar expandida e recolhida
        /// </summary>
        private void ToggleSidebar()
        {
            var storyboard = _sidebarExpanded
                ? (Storyboard)this.Resources["SidebarCollapse"]
                : (Storyboard)this.Resources["SidebarExpand"];

            storyboard?.Begin();
            _sidebarExpanded = !_sidebarExpanded;
        }

        #endregion
    }
}