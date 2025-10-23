using System.Threading.Tasks;
using System.Windows.Input;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Setup
{
    public class InitialSetupViewModel : BaseViewModel
    {
        private readonly ICompanyConfigService _companyService;
        private readonly IPersonService _personService;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPrivilegeService _privilegeService;
        private readonly IUserPrivilegeService _userPrivilegeService;
        private readonly AsyncCommand _saveCommand;

        public InitialSetupViewModel(
            ICompanyConfigService companyService,
            IPersonService personService,
            IUserService userService,
            INavigationService navigationService,
            IFileStorageService fileStorageService,
            IPrivilegeService privilegeService,
            IUserPrivilegeService userPrivilegeService)
        {
            _companyService = companyService;
            _personService = personService;
            _userService = userService;
            _navigationService = navigationService;
            _fileStorageService = fileStorageService;
            _privilegeService = privilegeService;
            _userPrivilegeService = userPrivilegeService;

            NextCommand = new RelayCommand(_ => NextStep(), _ => CanProceed);
            PreviousCommand = new RelayCommand(_ => PreviousStep(), _ => CanGoBack);

            Company = new CompanyConfigDto
            {
                Currency = "AOA",
                CurrencySymbol = "Kz",
                InvoicePrefix = "INV",
                NextInvoiceNumber = 1,
                IncludeCustomerData = true,
                AllowAnonymousInvoice = false,
                IsActive = true,
                DefaultTaxRate = 0
            };

            AdminPerson = new PersonDto { Type = 3, IsActive = true };
            AdminUser = new UserDto { Status = 1 };

            _saveCommand = new AsyncCommand(SaveAsync, CanSave, onStateChanged: OnCommandStateChanged);
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());

            // Inicializar propriedades de visibilidade
            UpdateStepVisibility();

            // Carregar privilégios ativos (não bloquear o construtor)
            _ = LoadPrivilegesAsync();
        }

        public CompanyConfigDto Company { get; set; }
        public PersonDto AdminPerson { get; set; }
        public UserDto AdminUser { get; set; }

        // Privileges selection
        public System.Collections.ObjectModel.ObservableCollection<PrivilegeSelectionItem> AvailablePrivileges { get; } = new System.Collections.ObjectModel.ObservableCollection<PrivilegeSelectionItem>();

        public class PrivilegeSelectionItem : System.ComponentModel.INotifyPropertyChanged
        {
            public PrivilegeDto Privilege { get; }
            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value) return;
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }

            public PrivilegeSelectionItem(PrivilegeDto p) => Privilege = p;
            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        }

        public async Task LoadPrivilegesAsync()
        {
            try
            {
                var list = await _privilegeService.GetActiveAsync();
                AvailablePrivileges.Clear();
                foreach (var p in list)
                {
                    AvailablePrivileges.Add(new PrivilegeSelectionItem(p));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar privilégios: {ex.Message}");
            }
        }

        // Senhas bindadas via code-behind
        public string AdminPassword { get; set; } = string.Empty;
        public string AdminConfirmPassword { get; set; } = string.Empty;

        // Propriedades para feedback de validação de senha
        private string _passwordValidationMessage = string.Empty;
        public string PasswordValidationMessage
        {
            get => _passwordValidationMessage;
            set => Set(ref _passwordValidationMessage, value);
        }

        private bool _passwordIsValid = false;
        public bool PasswordIsValid
        {
            get => _passwordIsValid;
            set => Set(ref _passwordIsValid, value);
        }

        // Propriedades para feedback de validação de username
        private string _usernameValidationMessage = string.Empty;
        public string UsernameValidationMessage
        {
            get => _usernameValidationMessage;
            set => Set(ref _usernameValidationMessage, value);
        }

        private bool _usernameIsValid = false;
        public bool UsernameIsValid
        {
            get => _usernameIsValid;
            set => Set(ref _usernameIsValid, value);
        }

        // Propriedade para mensagens de erro gerais
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        // Navegação entre etapas
        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (Set(ref _currentStep, value))
                {
                    UpdateStepVisibility();
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(IsNotLastStep));
                    OnPropertyChanged(nameof(IsLastStep));
                }
            }
        }

        // Propriedades de visibilidade dos steps
        private bool _isStep1Visible = true;
        public bool IsStep1Visible
        {
            get => _isStep1Visible;
            set => Set(ref _isStep1Visible, value);
        }

        private bool _isStep2Visible = false;
        public bool IsStep2Visible
        {
            get => _isStep2Visible;
            set => Set(ref _isStep2Visible, value);
        }

        private bool _isStep3Visible = false;
        public bool IsStep3Visible
        {
            get => _isStep3Visible;
            set => Set(ref _isStep3Visible, value);
        }

        // Propriedades de controle de navegação
        public bool CanGoBack => CurrentStep > 1;
        public bool IsNotLastStep => CurrentStep < 3;
        public bool IsLastStep => CurrentStep == 3;

        // Propriedade para habilitar/desabilitar botão Próximo
        private bool _canProceed = false;
        public bool CanProceed
        {
            get => _canProceed;
            set => Set(ref _canProceed, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (Set(ref _isBusy, value))
                {
                    _saveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand => _saveCommand;
        public ICommand ToggleThemeCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        // Métodos de validação
        public void ValidateCompanyStep()
        {
            CanProceed = !string.IsNullOrWhiteSpace(Company.CompanyName) &&
                        !string.IsNullOrWhiteSpace(Company.TaxId) &&
                        !string.IsNullOrWhiteSpace(Company.Email);
        }

        public void ValidatePersonStep()
        {
            CanProceed = !string.IsNullOrWhiteSpace(AdminPerson.Name) &&
                        !string.IsNullOrWhiteSpace(AdminPerson.Email);
        }

        public void ValidateUserStep()
        {
            // Validar username
            ValidateUsername();

            // Validar senhas
            ValidatePasswords();

            // CanProceed só é true se tudo estiver válido
            CanProceed = UsernameIsValid &&
                        PasswordIsValid &&
                        !string.IsNullOrWhiteSpace(AdminPassword) &&
                        AdminPassword == AdminConfirmPassword;
        }

        /// <summary>
        /// Valida o nome de usuário e atualiza feedback
        /// </summary>
        private void ValidateUsername()
        {
            if (string.IsNullOrWhiteSpace(AdminUser.Username))
            {
                UsernameIsValid = false;
                UsernameValidationMessage = "Nome de usuário é obrigatório";
                return;
            }

            if (!IsUsernameValid(AdminUser.Username))
            {
                UsernameIsValid = false;

                if (AdminUser.Username.Length < 3)
                    UsernameValidationMessage = "Mínimo 3 caracteres";
                else if (AdminUser.Username.Length > 100)
                    UsernameValidationMessage = "Máximo 100 caracteres";
                else
                    UsernameValidationMessage = "Apenas letras, números, pontos, hífens e underscores";

                return;
            }

            UsernameIsValid = true;
            UsernameValidationMessage = string.Empty;
        }

        /// <summary>
        /// Valida as senhas e atualiza feedback
        /// </summary>
        private void ValidatePasswords()
        {
            if (string.IsNullOrWhiteSpace(AdminPassword))
            {
                PasswordIsValid = false;
                PasswordValidationMessage = "Senha é obrigatória";
                return;
            }

            if (!IsPasswordValid(AdminPassword))
            {
                PasswordIsValid = false;
                PasswordValidationMessage = GetPasswordValidationError(AdminPassword);
                return;
            }

            if (AdminPassword != AdminConfirmPassword)
            {
                PasswordIsValid = false;
                PasswordValidationMessage = "As senhas não coincidem";
                return;
            }

            PasswordIsValid = true;
            PasswordValidationMessage = string.Empty;
        }

        /// <summary>
        /// Retorna mensagem de erro específica para a senha
        /// </summary>
        private string GetPasswordValidationError(string password)
        {
            if (password.Length < 8)
                return "Mínimo 8 caracteres";

            if (!password.Any(char.IsUpper))
                return "Deve conter pelo menos uma letra maiúscula";

            if (!password.Any(char.IsLower))
                return "Deve conter pelo menos uma letra minúscula";

            if (!password.Any(char.IsDigit))
                return "Deve conter pelo menos um número";

            return "Senha inválida";
        }

        /// <summary>
        /// Valida se a senha atende aos requisitos mínimos de segurança
        /// Consistente com User.ValidatePasswordStrength()
        /// </summary>
        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Mínimo 8 caracteres
            if (password.Length < 8)
                return false;

            // Pelo menos uma letra maiúscula
            if (!password.Any(char.IsUpper))
                return false;

            // Pelo menos uma letra minúscula
            if (!password.Any(char.IsLower))
                return false;

            // Pelo menos um número
            if (!password.Any(char.IsDigit))
                return false;

            return true;
        }

        /// <summary>
        /// Valida se o nome de usuário atende aos requisitos
        /// Consistente com User.ValidateUsername()
        /// </summary>
        private bool IsUsernameValid(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 3 || username.Length > 100)
                return false;

            // Apenas letras, números, pontos, hífens e underscores
            return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_');
        }

        private void UpdateStepVisibility()
        {
            IsStep1Visible = CurrentStep == 1;
            IsStep2Visible = CurrentStep == 2;
            IsStep3Visible = CurrentStep == 3;
        }

        public void ReevaluateCanSave() => _saveCommand.RaiseCanExecuteChanged();

        private bool CanSave()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(Company.CompanyName)
                && !string.IsNullOrWhiteSpace(AdminPerson.Name)
                && !string.IsNullOrWhiteSpace(AdminUser.Username)
                && !string.IsNullOrWhiteSpace(AdminPassword)
                && AdminPassword == AdminConfirmPassword;
        }

        public void NextStep()
        {
            if (CurrentStep < 3 && CanProceed)
            {
                CurrentStep++;
                CanProceed = false; // Reset para a próxima validação
            }
        }

        public void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;

                // Revalidar o step anterior
                switch (CurrentStep)
                {
                    case 1:
                        ValidateCompanyStep();
                        break;
                    case 2:
                        ValidatePersonStep();
                        break;
                }
            }
        }

        /// <summary>
        /// Salva a logo da empresa a partir de um caminho de origem.
        /// A View deve chamar este método após o utilizador selecionar o ficheiro.
        /// </summary>
        public async Task<bool> SaveLogoFromPathAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return false;

            try
            {
                IsBusy = true;

                // Remover logo anterior se existir
                if (!string.IsNullOrWhiteSpace(Company.LogoUrl) && System.IO.File.Exists(Company.LogoUrl))
                {
                    await _fileStorageService.DeleteFileAsync(Company.LogoUrl);
                }

                // Salvar novo ficheiro via serviço
                var savedPath = await _fileStorageService.SaveLogoAsync(sourcePath);

                // Atualizar DTO com o caminho definitivo
                Company.LogoUrl = savedPath;
                OnPropertyChanged(nameof(Company));

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar logo: {ex.Message}");
                // A View deve tratar e mostrar mensagem ao utilizador
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Remove a logo da empresa
        /// </summary>
        public async Task RemoveLogoAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Company.LogoUrl))
                {
                    await _fileStorageService.DeleteFileAsync(Company.LogoUrl);
                    Company.LogoUrl = string.Empty;
                    OnPropertyChanged(nameof(Company));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao remover logo: {ex.Message}");
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Validação final antes de salvar
                if (!IsPasswordValid(AdminPassword))
                {
                    throw new InvalidOperationException(
                        "A senha não atende aos requisitos mínimos:\n" +
                        "- Mínimo 8 caracteres\n" +
                        "- Pelo menos uma letra maiúscula\n" +
                        "- Pelo menos uma letra minúscula\n" +
                        "- Pelo menos um número");
                }

                if (!IsUsernameValid(AdminUser.Username))
                {
                    throw new InvalidOperationException(
                        "O nome de usuário deve ter entre 3 e 100 caracteres e " +
                        "conter apenas letras, números, pontos, hífens e underscores.");
                }

                if (AdminPassword != AdminConfirmPassword)
                {
                    throw new InvalidOperationException("As senhas não coincidem.");
                }

                // 1) Upsert da CompanyConfig
                var savedCompany = await _companyService.UpdateAsync(Company);

                // 2) Criar Person (Employee)
                AdminPerson.Type = 3; // Employee
                var savedPerson = await _personService.CreateAsync(AdminPerson);

                // 3) Registrar User vinculado à Person
                AdminUser.PersonId = savedPerson.PersonId;
                var savedUser = await _userService.RegisterAsync(AdminUser, AdminPassword);

                // 3.1) Associar privilégios selecionados
                try
                {
                    foreach (var item in AvailablePrivileges.Where(p => p.IsSelected))
                    {
                        var dto = new Core.DTOs.UserPrivilegeDto
                        {
                            UserId = savedUser.UserId,
                            PrivilegeId = item.Privilege.PrivilegeId,
                            GrantedAt = DateTime.UtcNow
                        };
                        await _userPrivilegeService.GrantAsync(dto);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao associar privilégios: {ex.Message}");
                    // não bloqueia o fluxo principal de criação do utilizador
                }

                // 4) Navegar para Login após configuração completa
                _navigationService.NavigateToLogin();
            }
            catch (ArgumentException ex)
            {
                // Erros de validação de senha/username vindos do UserService
                System.Diagnostics.Debug.WriteLine($"Erro de validação: {ex.Message}");

                // Mostrar mensagem ao usuário através de uma propriedade
                ErrorMessage = ex.Message;
                OnPropertyChanged(nameof(ErrorMessage));

                throw; // Re-throw para que a View possa capturar e mostrar
            }
            catch (InvalidOperationException ex)
            {
                // Username duplicado ou outras regras de negócio
                System.Diagnostics.Debug.WriteLine($"Erro de operação: {ex.Message}");

                ErrorMessage = ex.Message;
                OnPropertyChanged(nameof(ErrorMessage));

                throw;
            }
            catch (Exception ex)
            {
                // Outros erros inesperados
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");

                ErrorMessage = "Ocorreu um erro ao salvar a configuração. Por favor, tente novamente.";
                OnPropertyChanged(nameof(ErrorMessage));

                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnCommandStateChanged(bool executing)
        {
            IsBusy = executing;
        }

        private void ToggleTheme()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            var darkUri = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
            var lightUri = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");

            System.Windows.ResourceDictionary? darkRd = null;
            System.Windows.ResourceDictionary? lightRd = null;
            foreach (var md in app.Resources.MergedDictionaries)
            {
                if (md.Source == darkUri) darkRd = md;
                if (md.Source == lightUri) lightRd = md;
            }

            if (darkRd != null)
            {
                // currently dark -> switch to light
                app.Resources.MergedDictionaries.Remove(darkRd);
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = lightUri });
            }
            else if (lightRd != null)
            {
                // currently light -> switch to dark
                app.Resources.MergedDictionaries.Remove(lightRd);
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = darkUri });
            }
            else
            {
                // default -> add dark
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = darkUri });
            }
        }
    }
}
