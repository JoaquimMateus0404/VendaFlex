using System;
using System.Windows;

namespace VendaFlex.UI.Views.Main
{
    /// <summary>
    /// Lógica interna para SplashView.xaml
    /// </summary>
    public partial class SplashView : Window
    {
        public SplashView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Animação de fade in
            var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(Window.OpacityProperty, fade);
        }
    }
}
