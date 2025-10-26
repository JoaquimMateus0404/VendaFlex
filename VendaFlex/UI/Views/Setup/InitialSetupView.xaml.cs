using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VendaFlex.ViewModels.Setup;

namespace VendaFlex.UI.Views.Setup
{
    /// <summary>
    /// Code-behind para InitialSetupView.xaml
    /// Gerencia interações de UI, validações visuais e animações
    /// ✅ ATUALIZADO: Combinado com funcionalidades anteriores + novos campos
    /// </summary>
    public partial class InitialSetupView : Page
    {
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isDarkTheme = true; // Inicialmente Dark (App.xaml BundledTheme BaseTheme="Dark")

        public InitialSetupView()
        {
            InitializeComponent();
        }

        #region Page Lifecycle

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Focar no primeiro campo
            CompanyNameTextBox?.Focus();
        }

        #endregion

        #region Company Logo Management

        /// <summary>
        /// Upload do logo da empresa
        /// </summary>
        private async void UploadLogo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp|Todos os arquivos|*.*",
                Title = "Selecionar Logo da Empresa",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Carregar preview da imagem (responsabilidade da UI)
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Mostrar preview
                    LogoImage.Source = bitmap;
                    LogoImage.Visibility = Visibility.Visible;
                    RemoveLogoButton.Visibility = Visibility.Visible;

                    // Delegar salvamento ao ViewModel (lógica de negócio)
                    if (DataContext is InitialSetupViewModel vm)
                    {
                        var success = await vm.SaveLogoFromPathAsync(openFileDialog.FileName);
                        if (!success)
                        {
                            MessageBox.Show("Erro ao salvar o logo.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                            // Limpar preview se falhou
                            LogoImage.Source = null;
                            LogoImage.Visibility = Visibility.Collapsed;
                            RemoveLogoButton.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Erros de validação (tamanho, tipo de arquivo)
                    MessageBox.Show(ex.Message,
                        "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);

                    LogoImage.Source = null;
                    LogoImage.Visibility = Visibility.Collapsed;
                    RemoveLogoButton.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                    LogoImage.Source = null;
                    LogoImage.Visibility = Visibility.Collapsed;
                    RemoveLogoButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Remove o logo da empresa
        /// </summary>
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

        #endregion

        #region Person Profile Image Management

        /// <summary>
        /// Upload da foto de perfil do administrador
        /// </summary>
        private async void UploadProfileImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp|Todos os arquivos|*.*",
                Title = "Selecionar Foto de Perfil",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Carregar preview da imagem
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Mostrar preview
                    ProfileImage.Source = bitmap;
                    ProfileImage.Visibility = Visibility.Visible;
                    RemoveProfileImageButton.Visibility = Visibility.Visible;

                    // Delegar salvamento ao ViewModel
                    if (DataContext is InitialSetupViewModel vm)
                    {
                        var success = await vm.SaveProfileImageFromPathAsync(openFileDialog.FileName);
                        if (!success)
                        {
                            MessageBox.Show("Erro ao salvar a foto de perfil.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                            ProfileImage.Source = null;
                            ProfileImage.Visibility = Visibility.Collapsed;
                            RemoveProfileImageButton.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message,
                        "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);

                    ProfileImage.Source = null;
                    ProfileImage.Visibility = Visibility.Collapsed;
                    RemoveProfileImageButton.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

                    ProfileImage.Source = null;
                    ProfileImage.Visibility = Visibility.Collapsed;
                    RemoveProfileImageButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Remove a foto de perfil
        /// </summary>
        private async void RemoveProfileImage_Click(object sender, RoutedEventArgs e)
        {
            ProfileImage.Source = null;
            ProfileImage.Visibility = Visibility.Collapsed;
            RemoveProfileImageButton.Visibility = Visibility.Collapsed;

            if (DataContext is InitialSetupViewModel vm)
            {
                await vm.RemoveProfileImageAsync();
            }
        }

        #endregion

        #region Validation Event Handlers

        /// <summary>
        /// Valida campos da empresa quando há alterações
        /// </summary>
        private void ValidateCompanyFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidateCompanyStep();
            }
        }

        /// <summary>
        /// Valida campos da pessoa quando há alterações
        /// </summary>
        private void ValidatePersonFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidatePersonStep();
            }
        }

        /// <summary>
        /// Valida campos do usuário quando há alterações
        /// </summary>
        private void ValidateUserFields(object sender, TextChangedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.ValidateUserStep();
            }
        }

        #endregion

        #region Password Handling

        /// <summary>
        /// Captura alterações na senha e atualiza validação visual
        /// </summary>
        private void AdminPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _password = passwordBox.Password;
                UpdatePasswordStrength(_password);
                ValidatePasswordMatch();

                // Atualizar ViewModel
                if (DataContext is InitialSetupViewModel vm)
                {
                    vm.AdminPassword = passwordBox.Password;
                    vm.ValidateUserStep();
                    vm.ReevaluateCanSave();
                }
            }
        }

        /// <summary>
        /// Captura alterações na confirmação de senha
        /// </summary>
        private void AdminConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _confirmPassword = passwordBox.Password;
                ValidatePasswordMatch();

                // Atualizar ViewModel
                if (DataContext is InitialSetupViewModel vm)
                {
                    vm.AdminConfirmPassword = passwordBox.Password;
                    vm.ValidateUserStep();
                    vm.ReevaluateCanSave();
                }
            }
        }

        #endregion

        #region Password Strength Visual Feedback

        /// <summary>
        /// Atualiza indicador visual de força da senha com animações
        /// </summary>
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

            // Verificar requisitos individuais (8 caracteres mínimo)
            bool hasMinLength = password.Length >= 8;
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasNumber = password.Any(char.IsDigit);

            // Critérios de força
            if (password.Length >= 8) strength += 25;
            if (password.Length >= 12) strength += 15;
            if (password.Length >= 16) strength += 10;
            if (hasLower) strength += 15;
            if (hasUpper) strength += 15;
            if (hasNumber) strength += 20;

            // Atualizar ícones dos requisitos
            UpdateRequirementIcon(LengthIcon, LengthText, hasMinLength);
            UpdateRequirementIcon(UpperIcon, UpperText, hasUpper);
            UpdateRequirementIcon(LowerIcon, LowerText, hasLower);
            UpdateRequirementIcon(NumberIcon, NumberText, hasNumber);

            // Caractere especial (opcional, mas dá bonus)
            bool hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
            if (hasSpecial) strength += 15;
            // Não mostrar erro se não tiver caractere especial (é opcional)

            // Determinar texto e cor baseado na força
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

            // Animar texto de força
            PasswordStrengthText.Text = strengthText;
            var textColorAnimation = new ColorAnimation
            {
                To = strengthColor,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            var textBrush = new SolidColorBrush();
            textBrush.BeginAnimation(SolidColorBrush.ColorProperty, textColorAnimation);
            PasswordStrengthText.Foreground = textBrush;

            // Animar barra de progresso (largura)
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

        /// <summary>
        /// Atualiza ícone de requisito individual com cores
        /// </summary>
        private void UpdateRequirementIcon(PackIcon icon, TextBlock text, bool isMet)
        {
            if (isMet)
            {
                icon.Kind = PackIconKind.CheckCircle;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                text.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
            }
            else
            {
                icon.Kind = PackIconKind.Circle;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81)); // Dark gray
                text.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
            }
        }

        /// <summary>
        /// Valida se as senhas coincidem com feedback visual
        /// </summary>
        private void ValidatePasswordMatch()
        {
            if (string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_confirmPassword))
                return;

            // Atualizar ícone de match
            bool matches = _password == _confirmPassword;
            UpdateRequirementIcon(MatchIcon, MatchText, matches);

            if (!matches)
            {
                // Aplicar borda vermelha no campo de confirmação
                AdminConfirmPasswordBox.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                AdminConfirmPasswordBox.ToolTip = "As senhas não coincidem";
            }
            else
            {
                // Aplicar borda verde no campo de confirmação
                AdminConfirmPasswordBox.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                AdminConfirmPasswordBox.ToolTip = "As senhas coincidem";

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

        #endregion

        #region Navigation

        /// <summary>
        /// Navega para o próximo step
        /// </summary>
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

        /// <summary>
        /// Volta para o step anterior
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                vm.PreviousStep();
                UpdateStepIndicators(vm.CurrentStep);
                FocusFirstFieldOfCurrentStep(vm.CurrentStep);
            }
        }

        /// <summary>
        /// Atualiza indicadores visuais de step (círculos no header)
        /// </summary>
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

            // Step 3 (se necessário adicionar indicador visual)
            // Pode adicionar lógica similar para Step3Icon/Step3Number se existir
        }

        /// <summary>
        /// Foca no primeiro campo relevante de cada step
        /// </summary>
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

        #endregion

        #region Privilege Selection Helpers

        /// <summary>
        /// Seleciona todos os privilégios
        /// </summary>
        private void SelectAllPrivileges_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                foreach (var privilege in vm.AvailablePrivileges)
                {
                    privilege.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Desmarca todos os privilégios
        /// </summary>
        private void DeselectAllPrivileges_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialSetupViewModel vm)
            {
                foreach (var privilege in vm.AvailablePrivileges)
                {
                    privilege.IsSelected = false;
                }
            }
        }

        #endregion

        #region Theme Toggle

        /// <summary>
        /// Alterna entre tema claro e escuro
        /// </summary>
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            // Alternar tema usando BundledTheme nos recursos da aplicação
            var bundledTheme = Application.Current.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = _isDarkTheme ? BaseTheme.Light : BaseTheme.Dark;
                _isDarkTheme = !_isDarkTheme;
            }
        }

        #endregion
    }
}