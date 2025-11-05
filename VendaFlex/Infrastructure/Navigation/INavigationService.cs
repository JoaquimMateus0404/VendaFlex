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
        void NavigateToDashBoard();

        /// <summary>
        /// Navega para a tela de ponto de venda
        /// </summary>
        void NavigateToPdv();

        /// <summary>
        /// Navega para a tela de configurações da empresa
        /// </summary>
        void NavigateToCompanyConfig();


        /// <summary>
        /// Fecha a janela atual
        /// </summary>
        void CloseCurrentWindow();

        /// <summary>
        /// Define a janela atual
        /// </summary>
        void SetCurrentWindow(Window window);

        /// <summary>
        /// Navega para uma Page hospedada em uma nova Window com opções avançadas.
        /// </summary>
        void NavigateToPage<TView, TViewModel>(string title, NavigationOptions options)
            where TView : System.Windows.Controls.Page
            where TViewModel : class;
    }
}
