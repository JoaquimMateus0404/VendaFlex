using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converts a count (int/long or collection Count) to bool. True when > 0.
    /// Optional parameter: "invert" to invert the result.
    /// </summary>
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = TryGetCount(value);
            var result = count > 0;
            if (IsInvert(parameter)) result = !result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static int TryGetCount(object value)
        {
            switch (value)
            {
                case null:
                    return 0;
                case int i:
                    return i;
                case long l:
                    return (int)l;
                case short s:
                    return s;
                case string str:
                    return int.TryParse(str, NumberStyles.Any, provider: CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
                case IEnumerable enumerable:
                    // Try ICollection for O(1) count; otherwise enumerate.
                    if (enumerable is ICollection col)
                        return col.Count;
                    int c = 0; foreach (var _ in enumerable) c++; return c;
                default:
                    try
                    {
                        var prop = value.GetType().GetProperty("Count");
                        var val = prop?.GetValue(value);
                        if (val is int pi) return pi;
                        if (val is long pl) return (int)pl;
                    }
                    catch { /* ignore */ }
                    return 0;
            }
        }

        private static bool IsInvert(object parameter)
            => parameter is string s && s.Trim().Equals("invert", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a count to Visibility. Visible when > 0, else Collapsed.
    /// Optional parameter: "invert" to invert; "hidden" to use Hidden instead of Collapsed for the false state.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = 0;
            if (value != null)
            {
                if (value is int i) count = i;
                else if (value is long l) count = (int)l;
                else if (value is short s) count = s;
                else if (value is string str && int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) count = parsed;
                else if (value is IEnumerable enumerable)
                {
                    if (enumerable is ICollection col) count = col.Count;
                    else { foreach (var _ in enumerable) count++; }
                }
                else
                {
                    try
                    {
                        var prop = value.GetType().GetProperty("Count");
                        var v = prop?.GetValue(value);
                        if (v is int pi) count = pi;
                        else if (v is long pl) count = (int)pl;
                    }
                    catch { }
                }
            }

            var visible = count > 0;
            if (IsInvert(parameter)) visible = !visible;

            var falseIsHidden = IsHidden(parameter);
            return visible ? Visibility.Visible : (falseIsHidden ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static bool IsInvert(object parameter)
            => parameter is string s && s.IndexOf("invert", StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool IsHidden(object parameter)
            => parameter is string s && s.IndexOf("hidden", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Converts null to Collapsed and non-null to Visible.
    /// Optional parameter: "invert" to invert; "hidden" to use Hidden instead of Collapsed for the false state.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isNull = value == null;
            var invert = IsInvert(parameter);
            var falseIsHidden = IsHidden(parameter);
            var visible = invert ? isNull : !isNull;
            return visible ? Visibility.Visible : (falseIsHidden ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static bool IsInvert(object parameter)
            => parameter is string s && s.IndexOf("invert", StringComparison.OrdinalIgnoreCase) >= 0;
        private static bool IsHidden(object parameter)
            => parameter is string s && s.IndexOf("hidden", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
