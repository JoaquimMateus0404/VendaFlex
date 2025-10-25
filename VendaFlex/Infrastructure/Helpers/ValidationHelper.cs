using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VendaFlex.Infrastructure.Helpers
{
    /// <summary>
    /// Helper para exibir mensagens de validação em controles.
    /// Usa Attached Properties para vincular erros aos campos.
    /// </summary>
    public static class ValidationHelper
    {
        #region ValidationError Attached Property

        /// <summary>
        /// Mensagem de erro de validação.
        /// </summary>
        public static readonly DependencyProperty ValidationErrorProperty =
            DependencyProperty.RegisterAttached(
                "ValidationError",
                typeof(string),
                typeof(ValidationHelper),
                new PropertyMetadata(string.Empty, OnValidationErrorChanged));

        public static string GetValidationError(DependencyObject obj)
        {
            return (string)obj.GetValue(ValidationErrorProperty);
        }

        public static void SetValidationError(DependencyObject obj, string value)
        {
            obj.SetValue(ValidationErrorProperty, value);
        }

        private static void OnValidationErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                var error = e.NewValue as string;
                var hasError = !string.IsNullOrWhiteSpace(error);

                // Atualizar ToolTip
                element.ToolTip = hasError ? error : null;

                // Atualizar borda (se for TextBox, PasswordBox ou ComboBox)
                if (hasError)
                {
                    if (element is TextBox textBox)
                    {
                        textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                        textBox.BorderThickness = new Thickness(2);
                    }
                    else if (element is PasswordBox passwordBox)
                    {
                        passwordBox.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                        passwordBox.BorderThickness = new Thickness(2);
                    }
                    else if (element is ComboBox comboBox)
                    {
                        comboBox.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                        comboBox.BorderThickness = new Thickness(2);
                    }
                }
                else
                {
                    // Restaurar borda padrão
                    if (element is TextBox textBox)
                    {
                        textBox.ClearValue(TextBox.BorderBrushProperty);
                        textBox.ClearValue(TextBox.BorderThicknessProperty);
                    }
                    else if (element is PasswordBox passwordBox)
                    {
                        passwordBox.ClearValue(PasswordBox.BorderBrushProperty);
                        passwordBox.ClearValue(PasswordBox.BorderThicknessProperty);
                    }
                    else if (element is ComboBox comboBox)
                    {
                        comboBox.ClearValue(ComboBox.BorderBrushProperty);
                        comboBox.ClearValue(ComboBox.BorderThicknessProperty);
                    }
                }
            }
        }

        #endregion

        #region HasError Attached Property

        /// <summary>
        /// Indica se o controle tem erro.
        /// </summary>
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.RegisterAttached(
                "HasError",
                typeof(bool),
                typeof(ValidationHelper),
                new PropertyMetadata(false));

        public static bool GetHasError(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasErrorProperty);
        }

        public static void SetHasError(DependencyObject obj, bool value)
        {
            obj.SetValue(HasErrorProperty, value);
        }

        #endregion

        #region PropertyName Attached Property

        /// <summary>
        /// Nome da propriedade associada ao controle (para mapeamento de erros).
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(ValidationHelper),
                new PropertyMetadata(string.Empty));

        public static string GetPropertyName(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyNameProperty);
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        #endregion
    }
}