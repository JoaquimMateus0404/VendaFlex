using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converte string/URI em ImageSource e trata nulos/strings vazias retornando null sem gerar erros de binding.
    /// </summary>
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return null; // evita chamada ao ImageSourceConverter com null

                if (value is ImageSource img)
                    return img;

                var s = System.Convert.ToString(value, CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(s))
                    return null;

                // Tenta criar a partir de URI relativa/absoluta
                var uriKind = Uri.IsWellFormedUriString(s, UriKind.Absolute) ? UriKind.Absolute : UriKind.RelativeOrAbsolute;
                var uri = new Uri(s, uriKind);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = uri;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                // Em caso de erro (caminho inválido, etc.), retorna null para evitar exceção
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
