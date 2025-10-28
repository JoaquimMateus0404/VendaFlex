using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VendaFlex.UI.Views.Dashboard
{
    /// <summary>
    /// Code-behind para DashboardView com funcionalidades de popup
    /// ✅ Popups de notificações e perfil
    /// ✅ Hover effects nas linhas da tabela
    /// ✅ Animações suaves
    /// </summary>
    public partial class DashboardView : Page
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Iniciar animação de entrada
            var storyboard = (Storyboard)this.Resources["CardEntrance"];
            storyboard?.Begin();
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