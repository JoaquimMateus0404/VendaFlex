using System.Windows;

namespace VendaFlex.Infrastructure.Navigation
{
    public interface INavigationService
    {
        /// <summary>
        /// Navega para a tela de login
        /// </summary>
        void NavigateToLogin();

        /// <summary>
        /// Navega para a tela de configuração inicial
        /// </summary>
        void NavigateToSetup();

        /// <summary>
        /// Navega para a tela principal do sistema
        /// </summary>
        void NavigateToMain();

        /// <summary>
        /// Fecha a janela atual
        /// </summary>
        void CloseCurrentWindow();

        /// <summary>
        /// Define a janela atual
        /// </summary>
        void SetCurrentWindow(Window window);
    }
}
