using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VendaFlex.ViewModels.Setup;

namespace VendaFlex.UI.Views.Setup
{
    /// <summary>
    /// Interação lógica para InitialSetupView.xam
    /// </summary>
    public partial class InitialSetupView : Page
    {
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isDarkTheme = true; // Inicialmente Dark (App.xaml BundledTheme BaseTheme="Dark" )

        public InitialSetupView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Não há storyboard definido, mas podemos adicionar lógica futura aqui
            // Focar no primeiro campo
            CompanyNameTextBox.Focus();
        }

        private void AdminPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                _password = passwordBox.Password;
                UpdatePasswordStrength(_password);
                ValidatePasswordMatch();
            }

            if (DataContext is InitialSetupViewModel vm && sender is PasswordBox pb)
            {
                vm.AdminPassword = pb.Password;
                vm.ValidateUserStep(); // Revalidar o step
                vm.ReevaluateCanSave();
            }
        }

        private void AdminConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                _confirmPassword = passwordBox.Password;
                ValidatePasswordMatch();
            }

            if (DataContext is InitialSetupViewModel vm && sender is PasswordBox pb)
            {
                vm.AdminConfirmPassword = pb.Password;
                vm.ValidateUserStep(); // Revalidar o step
                vm.ReevaluateCanSave();
            }
        }

        // Validação de campos da empresa
        private void ValidateCompanyFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidateCompanyStep();
            }
        }

        // Validação de campos da pessoa
        private void ValidatePersonFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidatePersonStep();
            }
        }

        // Validação de campos do usuário
        private void ValidateUserFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidateUserStep();
            }
        }

        // Upload de logo
        private async void UploadLogo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png|Todos os arquivos|*.*",
                Title = "Selecionar Logo da Empresa",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Carregar preview da imagem (mantém responsabilidade da UI na View)
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Mostrar preview
                    LogoImage.Source = bitmap;
                    LogoImage.Visibility = Visibility.Visible;
                    RemoveLogoButton.Visibility = Visibility.Visible;

                    // Delegar salvamento ao ViewModel (responsabilidade de lógica de negócio)
                    if (DataContext is InitialSetupViewModel vm)
                    {
                        var success = await vm.SaveLogoFromPathAsync(openFileDialog.FileName);
                        if (!success)
                        {
                            MessageBox.Show("Erro ao salvar o logo.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Erros de validação (tamanho, tipo de ficheiro)
                    MessageBox.Show(ex.Message,
                        "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Limpar preview se falhou
                    LogoImage.Source = null;
                    LogoImage.Visibility = Visibility.Collapsed;
                    RemoveLogoButton.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Limpar preview se falhou
                    LogoImage.Source = null;
                    LogoImage.Visibility = Visibility.Collapsed;
                    RemoveLogoButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Remover logo
        private async void RemoveLogo_Click(object sender, RoutedEventArgs e)
        {
            LogoImage.Source = null;
            LogoImage.Visibility = Visibility.Collapsed;
            RemoveLogoButton.Visibility = Visibility.Collapsed;

            if (DataContext is InitialSetupViewModel vm)
            {
                await vm.RemoveLogoAsync();
            }
        }

        // Navegação - Botão Próximo
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.NextStep();

                // Atualizar indicadores visuais dos steps
                UpdateStepIndicators(vm.CurrentStep);

                // Focar no primeiro campo de cada step
                FocusFirstFieldOfCurrentStep(vm.CurrentStep);
            }
        }

        // Navegação - Botão Voltar
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.PreviousStep();
                UpdateStepIndicators(vm.CurrentStep);
                FocusFirstFieldOfCurrentStep(vm.CurrentStep);
            }
        }

        // Atualizar indicadores de step (círculos no header)
        private void UpdateStepIndicators(int currentStep)
        {
            // Step 1
            if (currentStep > 1)
            {
                Step1Icon.Visibility = Visibility.Visible;
                Step1Number.Visibility = Visibility.Collapsed;
            }
            else
            {
                Step1Icon.Visibility = Visibility.Collapsed;
                Step1Number.Visibility = Visibility.Visible;
            }

            // Step 2
            if (currentStep > 2)
            {
                Step2Icon.Visibility = Visibility.Visible;
                Step2Number.Visibility = Visibility.Collapsed;
            }
            else if (currentStep == 2)
            {
                Step2Icon.Visibility = Visibility.Collapsed;
                Step2Number.Visibility = Visibility.Visible;
            }
            else
            {
                Step2Icon.Visibility = Visibility.Collapsed;
                Step2Number.Visibility = Visibility.Visible;
            }
        }

        // Focar no primeiro campo de cada step
        private void FocusFirstFieldOfCurrentStep(int step)
        {
            switch (step)
            {
                case 1:
                    CompanyNameTextBox?.Focus();
                    break;
                case 2:
                    PersonNameTextBox?.Focus();
                    break;
                case 3:
                    UsernameTextBox?.Focus();
                    break;
            }
        }

        private void UpdatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            PasswordStrengthIndicator.Visibility = Visibility.Visible;

            // Calcular força da senha (0-100)
            int strength = 0;
            int maxWidth = 280; // Largura máxima da barra

            // Verificar requisitos individuais (atualizados para 8 caracteres)
            bool hasMinLength = password.Length >= 8; // ATUALIZADO: de 6 para 8
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasNumber = password.Any(char.IsDigit);

            // Critérios de força (ajustados)
            if (password.Length >= 8) strength += 25; // ATUALIZADO
            if (password.Length >= 12) strength += 15;
            if (password.Length >= 16) strength += 10;
            if (hasLower) strength += 15;
            if (hasUpper) strength += 15;
            if (hasNumber) strength += 20;

            // Atualizar ícones dos requisitos
            UpdateRequirementIcon(LengthIcon, LengthText, hasMinLength);
            UpdateRequirementIcon(UpperIcon, UpperText, hasUpper);
            UpdateRequirementIcon(NumberIcon, NumberText, hasNumber);

            // Remover requisito de caractere especial (não é obrigatório)
            // O ícone SpecialIcon ainda existe no XAML mas não vamos mostrar erro se não tiver
            bool hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
            if (hasSpecial) strength += 15; // Bonus se tiver
            UpdateRequirementIcon(SpecialIcon, SpecialText, hasSpecial);

            // Determinar texto e cor
            string strengthText;
            Color strengthColor;

            if (strength < 40)
            {
                strengthText = "Fraca";
                strengthColor = Color.FromRgb(239, 68, 68); // Red
            }
            else if (strength < 60)
            {
                strengthText = "Média";
                strengthColor = Color.FromRgb(251, 191, 36); // Yellow
            }
            else if (strength < 80)
            {
                strengthText = "Boa";
                strengthColor = Color.FromRgb(59, 130, 246); // Blue
            }
            else
            {
                strengthText = "Excelente";
                strengthColor = Color.FromRgb(16, 185, 129); // Green
            }

            // Animar texto
            PasswordStrengthText.Text = strengthText;
            var textColorAnimation = new ColorAnimation
            {
                To = strengthColor,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            var textBrush = new SolidColorBrush();
            textBrush.BeginAnimation(SolidColorBrush.ColorProperty, textColorAnimation);
            PasswordStrengthText.Foreground = textBrush;

            // Animar barra de progresso
            var widthAnimation = new DoubleAnimation
            {
                To = maxWidth * (strength / 100.0),
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            PasswordStrengthBar.BeginAnimation(WidthProperty, widthAnimation);

            // Animar cor da barra
            var barColorAnimation = new ColorAnimation
            {
                To = strengthColor,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            var barBrush = new SolidColorBrush();
            barBrush.BeginAnimation(SolidColorBrush.ColorProperty, barColorAnimation);
            PasswordStrengthBar.Background = barBrush;
        }

        // Atualizar ícone de requisito
        private void UpdateRequirementIcon(PackIcon icon, TextBlock text, bool isMet)
        {
            if (isMet)
            {
                icon.Kind = PackIconKind.CheckCircle;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                text.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175)); // Gray
            }
            else
            {
                icon.Kind = PackIconKind.Circle;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)); // Dark gray
                text.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
            }
        }

        private void ValidatePasswordMatch()
        {
            if (string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_confirmPassword))
                return;

            if (_password != _confirmPassword)
            {
                // Aplicar borda vermelha no campo de confirmação
                AdminConfirmPasswordBox.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                AdminConfirmPasswordBox.ToolTip = "As senhas não coincidem";
            }
            else
            {
                // Aplicar borda verde no campo de confirmação
                AdminConfirmPasswordBox.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                AdminConfirmPasswordBox.ToolTip = "As senhas coincidêm";

                // Animar feedback positivo
                var scaleTransform = new ScaleTransform(1, 1);
                AdminConfirmPasswordBox.RenderTransform = scaleTransform;
                AdminConfirmPasswordBox.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleUp = new DoubleAnimation
                {
                    To = 1.02,
                    Duration = TimeSpan.FromMilliseconds(100),
                    AutoReverse = true
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            // Alternar tema atuando diretamente sobre o BundledTheme nos recursos da aplicação,
            // evitando depender de chaves específicas no PaletteHelper.GetTheme().
            var bundledTheme = Application.Current.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = _isDarkTheme ? BaseTheme.Light : BaseTheme.Dark;
                _isDarkTheme = !_isDarkTheme;
            }
        }
    }
}
