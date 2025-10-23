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

        /// <summary>
        /// Navega para uma Page hospedada em uma nova Window.
        /// TView deve ser um System.Windows.Controls.Page e TViewModel o seu ViewModel.
        /// </summary>
        void NavigateToPage<TView, TViewModel>(string title, double width = 1000, double height = 700, bool closeCurrent = true)
            where TView : System.Windows.Controls.Page
            where TViewModel : class;
    }
}
