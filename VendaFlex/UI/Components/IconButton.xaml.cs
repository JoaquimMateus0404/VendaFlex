using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace VendaFlex.UI.Views.Components
{
    public partial class IconButton : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(IconButton),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(IconButton),
                new PropertyMetadata(PackIconKind.None, OnIconChanged));

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(nameof(ButtonStyle), typeof(string), typeof(IconButton),
                new PropertyMetadata("Primary"));

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(IconButton));

        public IconButton()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string ButtonStyle
        {
            get => (string)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconButton control)
            {
                control.TextElement.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconButton control && e.NewValue is PackIconKind kind && kind != PackIconKind.None)
            {
                control.IconElement.Kind = kind;
                control.IconElement.Visibility = Visibility.Visible;
            }
        }

        private void RootButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}
