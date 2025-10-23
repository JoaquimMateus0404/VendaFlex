using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;

namespace VendaFlex.UI.Views.Components
{
    public partial class ToastNotification : UserControl
    {
        public enum ToastType
        {
            Success,
            Error,
            Warning,
            Info
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ToastNotification),
                new PropertyMetadata("Notification", OnTitleChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ToastNotification),
                new PropertyMetadata("This is a notification message", OnMessageChanged));

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(ToastType), typeof(ToastNotification),
                new PropertyMetadata(ToastType.Info, OnTypeChanged));

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(nameof(Duration), typeof(int), typeof(ToastNotification),
                new PropertyMetadata(4000));

        public ToastNotification()
        {
            InitializeComponent();
            Loaded += ToastNotification_Loaded;
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public ToastType Type
        {
            get => (ToastType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public int Duration
        {
            get => (int)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification control)
            {
                control.TitleText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification control)
            {
                control.MessageText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToastNotification control && e.NewValue is ToastType type)
            {
                control.UpdateAppearance(type);
            }
        }

        private void UpdateAppearance(ToastType type)
        {
            var color = Application.Current.Resources["ColorPrimary"];
            PackIconKind icon = PackIconKind.Information;

            switch (type)
            {
                case ToastType.Success:
                    color = Application.Current.Resources["ColorSuccess"];
                    icon = PackIconKind.CheckCircle;
                    break;
                case ToastType.Error:
                    color = Application.Current.Resources["ColorError"];
                    icon = PackIconKind.AlertCircle;
                    break;
                case ToastType.Warning:
                    color = Application.Current.Resources["ColorWarning"];
                    icon = PackIconKind.Alert;
                    break;
                case ToastType.Info:
                    color = Application.Current.Resources["ColorInfo"];
                    icon = PackIconKind.InformationOutline;
                    break;
            }

            IconBorder.Background = new SolidColorBrush((Color)color);
            ToastIcon.Kind = icon;
        }

        private void ToastNotification_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto close after duration
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Duration)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            var storyboard = (Storyboard)Resources["SlideOutAnimation"];
            storyboard.Begin();
        }

        private void SlideOutAnimation_Completed(object sender, EventArgs e)
        {
            // Remove from parent
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
        }
    }
}
