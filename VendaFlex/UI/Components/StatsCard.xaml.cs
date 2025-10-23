using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace VendaFlex.UI.Views.Components
{
    public partial class StatsCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(StatsCard),
                new PropertyMetadata("Total Sales", OnTitleChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatsCard),
                new PropertyMetadata("0", OnValueChanged));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(StatsCard),
                new PropertyMetadata("vs. last month", OnSubtitleChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(StatsCard),
                new PropertyMetadata(PackIconKind.TrendingUp, OnIconChanged));

        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(StatsCard),
                new PropertyMetadata(null, OnIconColorChanged));

        public static readonly DependencyProperty TrendProperty =
            DependencyProperty.Register(nameof(Trend), typeof(string), typeof(StatsCard),
                new PropertyMetadata(null, OnTrendChanged));

        public static readonly DependencyProperty TrendDirectionProperty =
            DependencyProperty.Register(nameof(TrendDirection), typeof(TrendType), typeof(StatsCard),
                new PropertyMetadata(TrendType.Up, OnTrendDirectionChanged));

        public enum TrendType
        {
            Up,
            Down,
            Neutral
        }

        public StatsCard()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public string Trend
        {
            get => (string)GetValue(TrendProperty);
            set => SetValue(TrendProperty, value);
        }

        public TrendType TrendDirection
        {
            get => (TrendType)GetValue(TrendDirectionProperty);
            set => SetValue(TrendDirectionProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control)
            {
                control.TitleText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control)
            {
                control.ValueText.Text = e.NewValue?.ToString() ?? "0";
            }
        }

        private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control)
            {
                control.SubtitleText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control && e.NewValue is PackIconKind kind)
            {
                control.CardIcon.Kind = kind;
            }
        }

        private static void OnIconColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control && e.NewValue is Brush brush)
            {
                control.CardIcon.Foreground = brush;
                control.IconContainer.Background = new SolidColorBrush(((SolidColorBrush)brush).Color) 
                { Opacity = 0.15 };
            }
        }

        private static void OnTrendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control)
            {
                var trend = e.NewValue?.ToString();
                if (!string.IsNullOrEmpty(trend))
                {
                    control.TrendText.Text = trend;
                    control.TrendBadge.Visibility = Visibility.Visible;
                }
                else
                {
                    control.TrendBadge.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static void OnTrendDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatsCard control && e.NewValue is TrendType type)
            {
                var successBrush = (Brush)Application.Current.Resources["SuccessBrush"];
                var errorBrush = (Brush)Application.Current.Resources["ErrorBrush"];
                var textSecondaryBrush = (Brush)Application.Current.Resources["TextSecondaryBrush"];

                switch (type)
                {
                    case TrendType.Up:
                        control.TrendBadge.Background = successBrush;
                        control.TrendIcon.Kind = PackIconKind.TrendingUp;
                        break;
                    case TrendType.Down:
                        control.TrendBadge.Background = errorBrush;
                        control.TrendIcon.Kind = PackIconKind.TrendingDown;
                        break;
                    case TrendType.Neutral:
                        control.TrendBadge.Background = textSecondaryBrush;
                        control.TrendIcon.Kind = PackIconKind.Minus;
                        break;
                }
            }
        }
    }
}
