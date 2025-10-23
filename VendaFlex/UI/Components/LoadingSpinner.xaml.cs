using System.Windows;
using System.Windows.Controls;

namespace VendaFlex.UI.Views.Components
{
    public partial class LoadingSpinner : UserControl
    {
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(double), typeof(LoadingSpinner),
                new PropertyMetadata(100.0));

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(LoadingSpinner),
                new PropertyMetadata(false, OnShowIconChanged));

        public LoadingSpinner()
        {
            InitializeComponent();
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingSpinner control)
            {
                control.CenterIcon.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
