using System.Collections.ObjectModel;
using System.Windows.Input;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Setup
{
    /// <summary>
    /// ViewModel para configuração inicial do sistema.
    /// Gerencia wizard de 3 etapas: Empresa, Pessoa e Usuário.
    /// ✅ COMPLETO: Inclui todas as propriedades de erro para validação visual.
    /// </summary>
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

            // Inicializar comandos
            NextCommand = new RelayCommand(_ => NextStep(), _ => CanProceed);
            PreviousCommand = new RelayCommand(_ => PreviousStep(), _ => CanGoBack);
            _saveCommand = new AsyncCommand(SaveAsync, CanSave, onStateChanged: OnCommandStateChanged);
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());

            // Inicializar DTOs com valores padrão
            Company = new CompanyConfigDto
            {
                Currency = "AOA",
                CurrencySymbol = "Kz",
                InvoicePrefix = "INV",
                NextInvoiceNumber = 1,
                IncludeCustomerData = true,
                AllowAnonymousInvoice = false,
                IsActive = true,
                DefaultTaxRate = 0,
                InvoiceFormat = (int)CompanyConfig.InvoiceFormatType.A4
            };

            AdminPerson = new PersonDto
            {
                Type = (int)PersonType.Employee,
                IsActive = true
            };

            AdminUser = new UserDto
            {
                Status = (int)LoginStatus.Active
            };

            // Atualizar visibilidade dos steps
            UpdateStepVisibility();

            // Carregar privilégios
            _ = LoadPrivilegesAsync();
        }

        #region Properties

        public CompanyConfigDto Company { get; set; }
        public PersonDto AdminPerson { get; set; }
        public UserDto AdminUser { get; set; }

        // Senhas (não fazem parte do DTO)
        public string AdminPassword { get; set; } = string.Empty;
        public string AdminConfirmPassword { get; set; } = string.Empty;

        // Privilégios disponíveis
        public ObservableCollection<PrivilegeSelectionItem> AvailablePrivileges { get; } = new();

        /// <summary>
        /// Retorna a contagem de privilégios selecionados.
        /// </summary>
        public int SelectedPrivilegesCount => AvailablePrivileges.Count(p => p.IsSelected);

        public class PrivilegeSelectionItem : System.ComponentModel.INotifyPropertyChanged
        {
            private readonly InitialSetupViewModel? _viewModel;
            
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
                    
                    // Notify parent ViewModel that the count has changed
                    _viewModel?.OnPropertyChanged(nameof(SelectedPrivilegesCount));
                }
            }

            public PrivilegeSelectionItem(PrivilegeDto p, InitialSetupViewModel? viewModel = null)
            {
                Privilege = p;
                _viewModel = viewModel;
            }
            
            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        }

        #endregion

        #region Validation Errors - STEP 1 (Company)

        private string _companyNameError = string.Empty;
        public string CompanyNameError
        {
            get => _companyNameError;
            set => Set(ref _companyNameError, value);
        }

        private string _industryTypeError = string.Empty;
        public string IndustryTypeError
        {
            get => _industryTypeError;
            set => Set(ref _industryTypeError, value);
        }

        private string _taxRegimeError = string.Empty;
        public string TaxRegimeError
        {
            get => _taxRegimeError;
            set => Set(ref _taxRegimeError, value);
        }

        private string _taxIdError = string.Empty;
        public string TaxIdError
        {
            get => _taxIdError;
            set => Set(ref _taxIdError, value);
        }

        private string _emailError = string.Empty;
        public string EmailError
        {
            get => _emailError;
            set => Set(ref _emailError, value);
        }

        private string _phoneNumberError = string.Empty;
        public string PhoneNumberError
        {
            get => _phoneNumberError;
            set => Set(ref _phoneNumberError, value);
        }

        private string _websiteError = string.Empty;
        public string WebsiteError
        {
            get => _websiteError;
            set => Set(ref _websiteError, value);
        }

        private string _addressError = string.Empty;
        public string AddressError
        {
            get => _addressError;
            set => Set(ref _addressError, value);
        }

        private string _cityError = string.Empty;
        public string CityError
        {
            get => _cityError;
            set => Set(ref _cityError, value);
        }

        private string _postalCodeError = string.Empty;
        public string PostalCodeError
        {
            get => _postalCodeError;
            set => Set(ref _postalCodeError, value);
        }

        private string _countryError = string.Empty;
        public string CountryError
        {
            get => _countryError;
            set => Set(ref _countryError, value);
        }

        private string _currencyError = string.Empty;
        public string CurrencyError
        {
            get => _currencyError;
            set => Set(ref _currencyError, value);
        }

        private string _defaultTaxRateError = string.Empty;
        public string DefaultTaxRateError
        {
            get => _defaultTaxRateError;
            set => Set(ref _defaultTaxRateError, value);
        }

        private string _invoiceFormatError = string.Empty;
        public string InvoiceFormatError
        {
            get => _invoiceFormatError;
            set => Set(ref _invoiceFormatError, value);
        }

        private string _invoicePrefixError = string.Empty;
        public string InvoicePrefixError
        {
            get => _invoicePrefixError;
            set => Set(ref _invoicePrefixError, value);
        }

        private string _nextInvoiceNumberError = string.Empty;
        public string NextInvoiceNumberError
        {
            get => _nextInvoiceNumberError;
            set => Set(ref _nextInvoiceNumberError, value);
        }

        private string _invoiceFooterTextError = string.Empty;
        public string InvoiceFooterTextError
        {
            get => _invoiceFooterTextError;
            set => Set(ref _invoiceFooterTextError, value);
        }

        private string _businessHoursError = string.Empty;
        public string BusinessHoursError
        {
            get => _businessHoursError;
            set => Set(ref _businessHoursError, value);
        }

        #endregion

        #region Validation Errors - STEP 2 (Person)

        private string _personNameError = string.Empty;
        public string PersonNameError
        {
            get => _personNameError;
            set => Set(ref _personNameError, value);
        }

        private string _personTypeError = string.Empty;
        public string PersonTypeError
        {
            get => _personTypeError;
            set => Set(ref _personTypeError, value);
        }

        private string _personTaxIdError = string.Empty;
        public string PersonTaxIdError
        {
            get => _personTaxIdError;
            set => Set(ref _personTaxIdError, value);
        }

        private string _identificationNumberError = string.Empty;
        public string IdentificationNumberError
        {
            get => _identificationNumberError;
            set => Set(ref _identificationNumberError, value);
        }

        private string _personEmailError = string.Empty;
        public string PersonEmailError
        {
            get => _personEmailError;
            set => Set(ref _personEmailError, value);
        }

        private string _personPhoneNumberError = string.Empty;
        public string PersonPhoneNumberError
        {
            get => _personPhoneNumberError;
            set => Set(ref _personPhoneNumberError, value);
        }

        private string _mobileNumberError = string.Empty;
        public string MobileNumberError
        {
            get => _mobileNumberError;
            set => Set(ref _mobileNumberError, value);
        }

        private string _personWebsiteError = string.Empty;
        public string PersonWebsiteError
        {
            get => _personWebsiteError;
            set => Set(ref _personWebsiteError, value);
        }

        private string _personAddressError = string.Empty;
        public string PersonAddressError
        {
            get => _personAddressError;
            set => Set(ref _personAddressError, value);
        }

        private string _personCityError = string.Empty;
        public string PersonCityError
        {
            get => _personCityError;
            set => Set(ref _personCityError, value);
        }

        private string _personStateError = string.Empty;
        public string PersonStateError
        {
            get => _personStateError;
            set => Set(ref _personStateError, value);
        }

        private string _personPostalCodeError = string.Empty;
        public string PersonPostalCodeError
        {
            get => _personPostalCodeError;
            set => Set(ref _personPostalCodeError, value);
        }

        private string _personCountryError = string.Empty;
        public string PersonCountryError
        {
            get => _personCountryError;
            set => Set(ref _personCountryError, value);
        }

        private string _contactPersonError = string.Empty;
        public string ContactPersonError
        {
            get => _contactPersonError;
            set => Set(ref _contactPersonError, value);
        }

        private string _notesError = string.Empty;
        public string NotesError
        {
            get => _notesError;
            set => Set(ref _notesError, value);
        }

        #endregion

        #region Validation Errors - STEP 3 (User)

        private string _usernameError = string.Empty;
        public string UsernameError
        {
            get => _usernameError;
            set => Set(ref _usernameError, value);
        }

        private string _passwordError = string.Empty;
        public string PasswordError
        {
            get => _passwordError;
            set => Set(ref _passwordError, value);
        }

        private string _confirmPasswordError = string.Empty;
        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => Set(ref _confirmPasswordError, value);
        }

        #endregion

        #region Validation Feedback Properties

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

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set => Set(ref _successMessage, value);
        }

        #endregion

        #region Navigation Properties

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

        public bool CanGoBack => CurrentStep > 1;
        public bool IsNotLastStep => CurrentStep < 3;
        public bool IsLastStep => CurrentStep == 3;

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

        #endregion

        #region Commands

        public ICommand SaveCommand => _saveCommand;
        public ICommand ToggleThemeCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Carrega privilégios ativos do sistema.
        /// </summary>
        private async Task LoadPrivilegesAsync()
        {
            try
            {
                var result = await _privilegeService.GetActiveAsync();

                if (result is { Success: true, Data: not null })
                {
                    AvailablePrivileges.Clear();
                    foreach (var privilege in result.Data)
                    {
                        AvailablePrivileges.Add(new PrivilegeSelectionItem(privilege, this));
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao carregar privilégios: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar privilégios: {ex.Message}");
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Valida dados da etapa de Empresa.
        /// </summary>
        public void ValidateCompanyStep()
        {
            // Limpar erros anteriores
            ClearCompanyErrors();

            bool isValid = true;

            // Validar CompanyName (obrigatório)
            if (string.IsNullOrWhiteSpace(Company.CompanyName))
            {
                CompanyNameError = "Nome da empresa é obrigatório";
                isValid = false;
            }
            else if (Company.CompanyName.Length > 200)
            {
                CompanyNameError = "Máximo 200 caracteres";
                isValid = false;
            }

            // Validar Email (obrigatório)
            if (string.IsNullOrWhiteSpace(Company.Email))
            {
                EmailError = "Email é obrigatório";
                isValid = false;
            }
            else if (!IsValidEmail(Company.Email))
            {
                EmailError = "Email inválido";
                isValid = false;
            }

            // Validar TaxId
            if (!string.IsNullOrWhiteSpace(Company.TaxId) && Company.TaxId.Length > 50)
            {
                TaxIdError = "Máximo 50 caracteres";
                isValid = false;
            }

            // Validar Currency (obrigatório)
            if (string.IsNullOrWhiteSpace(Company.Currency))
            {
                CurrencyError = "Moeda é obrigatória";
                isValid = false;
            }

            CanProceed = isValid;
        }

        /// <summary>
        /// Valida dados da etapa de Pessoa.
        /// </summary>
        public void ValidatePersonStep()
        {
            // Limpar erros anteriores
            ClearPersonErrors();

            bool isValid = true;

            // Validar Name (obrigatório)
            if (string.IsNullOrWhiteSpace(AdminPerson.Name))
            {
                PersonNameError = "Nome é obrigatório";
                isValid = false;
            }
            else if (AdminPerson.Name.Length > 200)
            {
                PersonNameError = "Máximo 200 caracteres";
                isValid = false;
            }

            // Validar Email
            if (!string.IsNullOrWhiteSpace(AdminPerson.Email))
            {
                if (!IsValidEmail(AdminPerson.Email))
                {
                    PersonEmailError = "Email inválido";
                    isValid = false;
                }
                else if (AdminPerson.Email.Length > 200)
                {
                    PersonEmailError = "Máximo 200 caracteres";
                    isValid = false;
                }
            }

            CanProceed = isValid;
        }

        /// <summary>
        /// Valida dados da etapa de Usuário.
        /// </summary>
        public void ValidateUserStep()
        {
            ValidateUsername();
            ValidatePasswords();

            CanProceed = UsernameIsValid &&
                        PasswordIsValid &&
                        !string.IsNullOrWhiteSpace(AdminPassword) &&
                        AdminPassword == AdminConfirmPassword;
        }

        /// <summary>
        /// Valida o nome de usuário.
        /// </summary>
        private void ValidateUsername()
        {
            UsernameError = string.Empty;

            if (string.IsNullOrWhiteSpace(AdminUser.Username))
            {
                UsernameIsValid = false;
                UsernameValidationMessage = "Nome de usuário é obrigatório";
                UsernameError = "Campo obrigatório";
                return;
            }

            if (!User.ValidateUsername(AdminUser.Username))
            {
                UsernameIsValid = false;

                if (AdminUser.Username.Length < 3)
                {
                    UsernameValidationMessage = "Mínimo 3 caracteres";
                    UsernameError = "Mínimo 3 caracteres";
                }
                else if (AdminUser.Username.Length > 100)
                {
                    UsernameValidationMessage = "Máximo 100 caracteres";
                    UsernameError = "Máximo 100 caracteres";
                }
                else
                {
                    UsernameValidationMessage = "Apenas letras, números, pontos, hífens e underscores";
                    UsernameError = "Formato inválido";
                }

                return;
            }

            UsernameIsValid = true;
            UsernameValidationMessage = string.Empty;
            UsernameError = string.Empty;
        }

        /// <summary>
        /// Valida as senhas.
        /// </summary>
        private void ValidatePasswords()
        {
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;

            if (string.IsNullOrWhiteSpace(AdminPassword))
            {
                PasswordIsValid = false;
                PasswordValidationMessage = "Senha é obrigatória";
                PasswordError = "Campo obrigatório";
                return;
            }

            if (!User.ValidatePasswordStrength(AdminPassword))
            {
                PasswordIsValid = false;
                PasswordValidationMessage = GetPasswordValidationError(AdminPassword);
                PasswordError = "Senha fraca";
                return;
            }

            if (AdminPassword != AdminConfirmPassword)
            {
                PasswordIsValid = false;
                PasswordValidationMessage = "As senhas não coincidem";
                ConfirmPasswordError = "Senhas não coincidem";
                return;
            }

            PasswordIsValid = true;
            PasswordValidationMessage = string.Empty;
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
        }

        /// <summary>
        /// Retorna mensagem de erro específica para a senha.
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
        /// Valida formato de email.
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Limpa erros de validação da empresa.
        /// </summary>
        private void ClearCompanyErrors()
        {
            CompanyNameError = string.Empty;
            IndustryTypeError = string.Empty;
            TaxRegimeError = string.Empty;
            TaxIdError = string.Empty;
            EmailError = string.Empty;
            PhoneNumberError = string.Empty;
            WebsiteError = string.Empty;
            AddressError = string.Empty;
            CityError = string.Empty;
            PostalCodeError = string.Empty;
            CountryError = string.Empty;
            CurrencyError = string.Empty;
            DefaultTaxRateError = string.Empty;
            InvoiceFormatError = string.Empty;
            InvoicePrefixError = string.Empty;
            NextInvoiceNumberError = string.Empty;
            InvoiceFooterTextError = string.Empty;
            BusinessHoursError = string.Empty;
        }

        /// <summary>
        /// Limpa erros de validação da pessoa.
        /// </summary>
        private void ClearPersonErrors()
        {
            PersonNameError = string.Empty;
            PersonTypeError = string.Empty;
            PersonTaxIdError = string.Empty;
            IdentificationNumberError = string.Empty;
            PersonEmailError = string.Empty;
            PersonPhoneNumberError = string.Empty;
            MobileNumberError = string.Empty;
            PersonWebsiteError = string.Empty;
            PersonAddressError = string.Empty;
            PersonCityError = string.Empty;
            PersonStateError = string.Empty;
            PersonPostalCodeError = string.Empty;
            PersonCountryError = string.Empty;
            ContactPersonError = string.Empty;
            NotesError = string.Empty;
        }

        #endregion

        #region Navigation Methods

        private void UpdateStepVisibility()
        {
            IsStep1Visible = CurrentStep == 1;
            IsStep2Visible = CurrentStep == 2;
            IsStep3Visible = CurrentStep == 3;
        }

        public void NextStep()
        {
            if (CurrentStep < 3 && CanProceed)
            {
                CurrentStep++;
                CanProceed = false; // Reset para próxima validação
                ErrorMessage = string.Empty;
            }
        }

        public void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
                ErrorMessage = string.Empty;

                // Revalidar step anterior
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

        #endregion

        #region Logo Management

        /// <summary>
        /// Salva o logo da empresa a partir de um caminho.
        /// </summary>
        public async Task<bool> SaveLogoFromPathAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return false;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Remover logo anterior se existir
                if (!string.IsNullOrWhiteSpace(Company.LogoUrl) && System.IO.File.Exists(Company.LogoUrl))
                {
                    await _fileStorageService.DeleteFileAsync(Company.LogoUrl);
                }

                // Salvar novo arquivo
                var savedPath = await _fileStorageService.SaveLogoAsync(sourcePath);

                // Atualizar DTO
                Company.LogoUrl = savedPath;
                OnPropertyChanged(nameof(Company));

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar logo: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar logo: {ex.Message}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Remove o logo da empresa.
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

        /// <summary>
        /// Salva a foto de perfil do administrador.
        /// </summary>
        public async Task<bool> SaveProfileImageFromPathAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return false;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Remover imagem anterior se existir
                if (!string.IsNullOrWhiteSpace(AdminPerson.ProfileImageUrl) &&
                    System.IO.File.Exists(AdminPerson.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(AdminPerson.ProfileImageUrl);
                }

                // Salvar novo arquivo (usar método similar ao logo)
                var savedPath = await _fileStorageService.SaveLogoAsync(sourcePath);

                // Atualizar DTO
                AdminPerson.ProfileImageUrl = savedPath;
                OnPropertyChanged(nameof(AdminPerson));

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar foto: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar foto: {ex.Message}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Remove a foto de perfil.
        /// </summary>
        public async Task RemoveProfileImageAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(AdminPerson.ProfileImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(AdminPerson.ProfileImageUrl);
                    AdminPerson.ProfileImageUrl = string.Empty;
                    OnPropertyChanged(nameof(AdminPerson));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao remover foto: {ex.Message}");
            }
        }

        #endregion

        #region Save Operation

        public void ReevaluateCanSave() => _saveCommand.RaiseCanExecuteChanged();

        private bool CanSave()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(Company.CompanyName)
                && !string.IsNullOrWhiteSpace(AdminPerson.Name)
                && !string.IsNullOrWhiteSpace(AdminUser.Username)
                && !string.IsNullOrWhiteSpace(AdminPassword)
                && AdminPassword == AdminConfirmPassword
                && User.ValidatePasswordStrength(AdminPassword)
                && User.ValidateUsername(AdminUser.Username);
        }

        /// <summary>
        /// Salva toda a configuração inicial.
        /// ✅ CORRIGIDO: Usa OperationResult corretamente.
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // 1) Salvar configuração da empresa
                var companyResult = await _companyService.UpdateAsync(Company);

                if (!companyResult.Success)
                {
                    ErrorMessage = companyResult.Message ?? "Erro ao salvar configuração da empresa.";

                    if (companyResult.Errors?.Any() == true)
                        ErrorMessage += "\n• " + string.Join("\n• ", companyResult.Errors);

                    return;
                }

                // 2) Criar pessoa (funcionário)
                AdminPerson.Type = (int)PersonType.Employee;
                var personResult = await _personService.CreateAsync(AdminPerson);

                // ✅ VERIFICAÇÃO SEGURA
                if (!personResult.Success || personResult.Data == null)
                {
                    ErrorMessage = personResult.Message ?? "Erro ao criar pessoa.";

                    if (personResult.Errors?.Any() == true)
                        ErrorMessage += "\n• " + string.Join("\n• ", personResult.Errors);

                    return;
                }

                var createdPerson = personResult.Data;

                // 3) Registrar usuário vinculado à pessoa
                AdminUser.PersonId = createdPerson.PersonId;
                var userResult = await _userService.RegisterAsync(AdminUser, AdminPassword);

                // ✅ VERIFICAÇÃO SEGURA COM PATTERN MATCHING
                if (userResult is not { Success: true, Data: not null })
                {
                    ErrorMessage = userResult.Message ?? "Erro ao registrar usuário.";

                    if (userResult.Errors?.Any() == true)
                        ErrorMessage += "\n• " + string.Join("\n• ", userResult.Errors);

                    return;
                }

                var createdUser = userResult.Data;

                // 4) Associar privilégios selecionados
                await AssociatePrivilegesAsync(createdUser.UserId);

                // 5) Sucesso - navegar para login
                SuccessMessage = "Configuração inicial concluída com sucesso!";

                // Aguardar um pouco para usuário ver mensagem de sucesso
                await Task.Delay(1500);

                _navigationService.NavigateToLogin();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro inesperado: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Associa privilégios selecionados ao usuário.
        /// </summary>
        private async Task AssociatePrivilegesAsync(int userId)
        {
            try
            {
                var selectedPrivileges = AvailablePrivileges.Where(p => p.IsSelected).ToList();

                foreach (var item in selectedPrivileges)
                {
                    var dto = new UserPrivilegeDto
                    {
                        UserId = userId,
                        PrivilegeId = item.Privilege.PrivilegeId,
                        GrantedAt = DateTime.UtcNow
                    };

                    // ✅ USAR SERVICE COM OPERATIONRESULT
                    var result = await _userPrivilegeService.GrantAsync(dto);

                    if (!result.Success)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Erro ao associar privilégio {item.Privilege.Name}: {result.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao associar privilégios: {ex.Message}");
                // Não bloqueia o fluxo principal
            }
        }

        private void OnCommandStateChanged(bool executing)
        {
            IsBusy = executing;
        }

        #endregion

        #region Theme Toggle

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
                // Trocar de dark para light
                app.Resources.MergedDictionaries.Remove(darkRd);
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = lightUri });
            }
            else if (lightRd != null)
            {
                // Trocar de light para dark
                app.Resources.MergedDictionaries.Remove(lightRd);
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = darkUri });
            }
            else
            {
                // Adicionar dark como padrão
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary { Source = darkUri });
            }
        }

        #endregion
    }
}