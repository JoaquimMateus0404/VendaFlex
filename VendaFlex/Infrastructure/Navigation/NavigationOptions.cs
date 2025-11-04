using System.Windows;

namespace VendaFlex.Infrastructure.Navigation
{
    /// <summary>
    /// Modo de navegação entre janelas.
    /// </summary>
    public enum NavigationMode
    {
        /// <summary>
        /// Substitui a janela atual (fecha a anterior e define a nova como MainWindow)
        /// </summary>
        Replace = 0,

        /// <summary>
        /// Mantém a janela atual aberta e abre a nova (não modal)
        /// </summary>
        Stack = 1,

        /// <summary>
        /// Abre a nova janela como modal (bloqueia interação com a chamadora)
        /// </summary>
        Dialog = 2,

        /// <summary>
        /// Se já existir uma janela da mesma view, apenas traz para frente em vez de criar outra
        /// </summary>
        FocusExisting = 3
    }

    /// <summary>
    /// Opções avançadas para controlar a criação e exibição de janelas.
    /// </summary>
    public class NavigationOptions
    {
        public string? Title { get; set; }

        // Tamanho/posicionamento
        public double Width { get; set; } = 1000;
        public double Height { get; set; } = 700;
        public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.CenterScreen;

        // Estilo e comportamento da janela
        public WindowStyle? WindowStyle { get; set; } = System.Windows.WindowStyle.SingleBorderWindow;
        public ResizeMode? ResizeMode { get; set; } = System.Windows.ResizeMode.CanResize;
        public WindowState? WindowState { get; set; } = System.Windows.WindowState.Normal;
        public bool? ShowInTaskbar { get; set; } = true;
        public bool? Topmost { get; set; } = false;

        // Modo de navegação
        public NavigationMode Mode { get; set; } = NavigationMode.Replace;

        // Particularidades
        public bool SetAsMainWindow { get; set; } = true; // aplicável quando Mode != Dialog

        // Quando Mode == Dialog
        public bool IsModal => Mode == NavigationMode.Dialog;
    }
}
