using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Settings
{
    /// <summary>
    /// ViewModel para gerenciamento das configurações da empresa.
    /// Permite visualizar e editar todas as configurações globais do sistema. 
    /// </summary>
    public class CompanyConfigViewModel : BaseViewModel
    {
        private readonly ICompanyConfigService _companyConfigService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly DispatcherTimer _statusMessageTimer;
        private int _originalConfigId;

        #region Properties - Informações Gerais

        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set
            {
                if (Set(ref _companyName, value))
                {
                    HasChanges = true;
                    ValidateCompanyName();
                }
            }
        }

        private string? _industryType;
        public string? IndustryType
        {
            get => _industryType;
            set
            {
                if (Set(ref _industryType, value))
                    HasChanges = true;
            }
        }

        private string? _taxRegime;
        public string? TaxRegime
        {
            get => _taxRegime;
            set
            {
                if (Set(ref _taxRegime, value))
                    HasChanges = true;
            }
        }

        private string? _taxId;
        public string? TaxId
        {
            get => _taxId;
            set
            {
                if (Set(ref _taxId, value))
                    HasChanges = true;
            }
        }

        private string? _logoUrl;
        public string? LogoUrl
        {
            get => _logoUrl;
            set
            {
                if (Set(ref _logoUrl, value))
                {
                    HasChanges = true;
                    OnPropertyChanged(nameof(HasLogo));
                }
            }
        }

        public bool HasLogo => !string.IsNullOrWhiteSpace(LogoUrl);

        private string? _businessHours;
        public string? BusinessHours
        {
            get => _businessHours;
            set
            {
                if (Set(ref _businessHours, value))
                    HasChanges = true;
            }
        }

        #endregion

        #region Properties - Contato

        private string? _phoneNumber;
        public string? PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (Set(ref _phoneNumber, value))
                    HasChanges = true;
            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                if (Set(ref _email, value))
                {
                    HasChanges = true;
                    ValidateEmail();
                }
            }
        }

        private string? _website;
        public string? Website
        {
            get => _website;
            set
            {
                if (Set(ref _website, value))
                    HasChanges = true;
            }
        }

        #endregion

        #region Properties - Endereço

        private string? _address;
        public string? Address
        {
            get => _address;
            set
            {
                if (Set(ref _address, value))
                    HasChanges = true;
            }
        }

        private string? _city;
        public string? City
        {
            get => _city;
            set
            {
                if (Set(ref _city, value))
                    HasChanges = true;
            }
        }

        private string? _postalCode;
        public string? PostalCode
        {
            get => _postalCode;
            set
            {
                if (Set(ref _postalCode, value))
                    HasChanges = true;
            }
        }

        private string? _country;
        public string? Country
        {
            get => _country;
            set
            {
                if (Set(ref _country, value))
                    HasChanges = true;
            }
        }

        #endregion

        #region Properties - Configurações Fiscais

        private string _currency = "AOA";
        public string Currency
        {
            get => _currency;
            set
            {
                if (Set(ref _currency, value))
                    HasChanges = true;
            }
        }

        private string _currencySymbol = "Kz";
        public string CurrencySymbol
        {
            get => _currencySymbol;
            set
            {
                if (Set(ref _currencySymbol, value))
                    HasChanges = true;
            }
        }

        private decimal _defaultTaxRate;
        public decimal DefaultTaxRate
        {
            get => _defaultTaxRate;
            set
            {
                if (Set(ref _defaultTaxRate, value))
                {
                    HasChanges = true;
                    ValidateTaxRate();
                }
            }
        }

        #endregion

        #region Properties - Configurações de Fatura

        private string _invoicePrefix = "INV";
        public string InvoicePrefix
        {
            get => _invoicePrefix;
            set
            {
                if (Set(ref _invoicePrefix, value))
                    HasChanges = true;
            }
        }

        private int _nextInvoiceNumber = 1;
        public int NextInvoiceNumber
        {
            get => _nextInvoiceNumber;
            set
            {
                if (Set(ref _nextInvoiceNumber, value))
                {
                    HasChanges = true;
                    UpdateInvoicePreview();
                }
            }
        }

        private string? _invoiceFooterText;
        public string? InvoiceFooterText
        {
            get => _invoiceFooterText;
            set
            {
                if (Set(ref _invoiceFooterText, value))
                    HasChanges = true;
            }
        }

        private int _invoiceFormat;
        public int InvoiceFormat
        {
            get => _invoiceFormat;
            set
            {
                if (Set(ref _invoiceFormat, value))
                    HasChanges = true;
            }
        }

        private bool _includeCustomerData = true;
        public bool IncludeCustomerData
        {
            get => _includeCustomerData;
            set
            {
                if (Set(ref _includeCustomerData, value))
                    HasChanges = true;
            }
        }

        private bool _allowAnonymousInvoice;
        public bool AllowAnonymousInvoice
        {
            get => _allowAnonymousInvoice;
            set
            {
                if (Set(ref _allowAnonymousInvoice, value))
                    HasChanges = true;
            }
        }

        private string _invoicePreview = "INV-00001";
        public string InvoicePreview
        {
            get => _invoicePreview;
            set => Set(ref _invoicePreview, value);
        }

        #endregion

        #region Properties - UI State

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (Set(ref _isActive, value))
                    HasChanges = true;
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set => Set(ref _isSaving, value);
        }

        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            set => Set(ref _hasChanges, value);
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        private bool _isStatusError;
        public bool IsStatusError
        {
            get => _isStatusError;
            set => Set(ref _isStatusError, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => Set(ref _selectedTabIndex, value);
        }

        #endregion

        #region Validation Properties

        private string? _companyNameError;
        public string? CompanyNameError
        {
            get => _companyNameError;
            set => Set(ref _companyNameError, value);
        }

        private string? _emailError;
        public string? EmailError
        {
            get => _emailError;
            set => Set(ref _emailError, value);
        }

        private string? _taxRateError;
        public string? TaxRateError
        {
            get => _taxRateError;
            set => Set(ref _taxRateError, value);
        }

        public bool IsValid => string.IsNullOrEmpty(CompanyNameError) &&
                               string.IsNullOrEmpty(EmailError) &&
                               string.IsNullOrEmpty(TaxRateError);

        #endregion

        #region Collections

        public ObservableCollection<string> IndustryTypes { get; } = new()
        {
            "Varejo",
            "Atacado",
            "Serviços",
            "Alimentação",
            "Tecnologia",
            "Saúde",
            "Educação",
            "Construção",
            "Transporte",
            "Outro"
        };

        public ObservableCollection<string> TaxRegimes { get; } = new()
        {
            "Regime Geral",
            "Regime Simplificado",
            "Regime de Exclusão",
            "Outro"
        };

        public ObservableCollection<string> Countries { get; } = new()
        {
            "Angola",
            "Brasil",
            "Portugal",
            "Moçambique",
            "Cabo Verde",
            "Outro"
        };

        public ObservableCollection<CurrencyOption> Currencies { get; } = new()
        {
            new CurrencyOption { Code = "AOA", Name = "Kwanza Angolano", Symbol = "Kz" },
            new CurrencyOption { Code = "USD", Name = "Dólar Americano", Symbol = "$" },
            new CurrencyOption { Code = "EUR", Name = "Euro", Symbol = "€" },
            new CurrencyOption { Code = "BRL", Name = "Real Brasileiro", Symbol = "R$" },
        };

        private CurrencyOption? _selectedCurrency;
        public CurrencyOption? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (Set(ref _selectedCurrency, value) && value != null)
                {
                    Currency = value.Code;
                    CurrencySymbol = value.Symbol;
                }
            }
        }

        public ObservableCollection<InvoiceFormatOption> InvoiceFormats { get; } = new()
        {
            new InvoiceFormatOption { Value = 0, Name = "A4 (Papel)", Description = "Formato padrão para impressoras A4" },
            new InvoiceFormatOption { Value = 1, Name = "Rolo (Térmica)", Description = "Formato para impressoras térmicas" }
        };

        private InvoiceFormatOption? _selectedInvoiceFormat;
        public InvoiceFormatOption? SelectedInvoiceFormat
        {
            get => _selectedInvoiceFormat;
            set
            {
                if (Set(ref _selectedInvoiceFormat, value) && value != null)
                {
                    InvoiceFormat = value.Value;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadLogoCommand { get; }
        public ICommand RemoveLogoCommand { get; }
        public ICommand ActivateCommand { get; }
        public ICommand DeactivateCommand { get; }
        public ICommand TestInvoiceNumberCommand { get; }

        #endregion

        #region Constructor

        public CompanyConfigViewModel(
            ICompanyConfigService companyConfigService,
            ICurrentUserContext currentUserContext,
            IFileStorageService fileStorageService)
        {
            _companyConfigService = companyConfigService ?? throw new ArgumentNullException(nameof(companyConfigService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));

            // Initialize status message timer
            _statusMessageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _statusMessageTimer.Tick += (s, e) =>
            {
                StatusMessage = null;
                _statusMessageTimer.Stop();
            };

            // Initialize commands
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => HasChanges && IsValid && !IsSaving);
            CancelCommand = new RelayCommand(async _ => await CancelAsync(), _ => HasChanges);
            UploadLogoCommand = new RelayCommand(_ => UploadLogo());
            RemoveLogoCommand = new RelayCommand(async _ => await RemoveLogoAsync(), _ => HasLogo);
            ActivateCommand = new RelayCommand(async _ => await ActivateAsync(), _ => !IsActive);
            DeactivateCommand = new RelayCommand(async _ => await DeactivateAsync(), _ => IsActive);
            TestInvoiceNumberCommand = new RelayCommand(async _ => await TestInvoiceNumberAsync());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Carrega as configurações da empresa.
        /// </summary>
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = null;

                var result = await _companyConfigService.GetAsync();

                if (result.Success && result.Data != null)
                {
                    MapDtoToViewModel(result.Data);
                    HasChanges = false;
                    ShowStatusMessage("Configurações carregadas com sucesso!", false);
                }
                else
                {
                    ShowStatusMessage(result.Message ?? "Erro ao carregar configurações", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar configurações: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Salva as configurações da empresa.
        /// </summary>
        public async Task SaveAsync()
        {
            if (!ValidateAll())
            {
                ShowStatusMessage("Por favor, corrija os erros antes de salvar.", true);
                return;
            }

            try
            {
                IsSaving = true;
                StatusMessage = null;

                var dto = MapViewModelToDto();
                var result = await _companyConfigService.UpdateAsync(dto);

                if (result.Success)
                {
                    if (result.Data != null)
                    {
                        MapDtoToViewModel(result.Data);
                    }
                    HasChanges = false;
                    ShowStatusMessage("Configurações salvas com sucesso!", false);
                }
                else
                {
                    ShowStatusMessage(result.Message ?? "Erro ao salvar configurações", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao salvar configurações: {ex.Message}", true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Cancela as alterações e recarrega os dados originais.
        /// </summary>
        public async Task CancelAsync()
        {
            var result = MessageBox.Show(
                "Deseja descartar as alterações não salvas?",
                "Confirmar Cancelamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await LoadAsync();
            }
        }

        #endregion

        #region Private Methods

        private void MapDtoToViewModel(CompanyConfigDto dto)
        {
            _originalConfigId = dto.CompanyConfigId;
            CompanyName = dto.CompanyName ?? string.Empty;
            IndustryType = dto.IndustryType;
            TaxRegime = dto.TaxRegime;
            TaxId = dto.TaxId;
            Address = dto.Address;
            City = dto.City;
            PostalCode = dto.PostalCode;
            Country = dto.Country;
            PhoneNumber = dto.PhoneNumber;
            Email = dto.Email;
            Website = dto.Website;
            LogoUrl = dto.LogoUrl;
            Currency = dto.Currency ?? "AOA";
            CurrencySymbol = dto.CurrencySymbol ?? "Kz";
            DefaultTaxRate = dto.DefaultTaxRate;
            InvoiceFooterText = dto.InvoiceFooterText;
            InvoicePrefix = dto.InvoicePrefix ?? "INV";
            NextInvoiceNumber = dto.NextInvoiceNumber;
            InvoiceFormat = dto.InvoiceFormat;
            IncludeCustomerData = dto.IncludeCustomerData;
            AllowAnonymousInvoice = dto.AllowAnonymousInvoice;
            BusinessHours = dto.BusinessHours;
            IsActive = dto.IsActive;

            // Set selected items in combos
            SelectedCurrency = Currencies.FirstOrDefault(c => c.Code == Currency);
            SelectedInvoiceFormat = InvoiceFormats.FirstOrDefault(f => f.Value == InvoiceFormat);

            UpdateInvoicePreview();
        }

        private CompanyConfigDto MapViewModelToDto()
        {
            return new CompanyConfigDto
            {
                CompanyConfigId = _originalConfigId,
                CompanyName = CompanyName,
                IndustryType = IndustryType!,
                TaxRegime = TaxRegime!,
                TaxId = TaxId!,
                Address = Address!,
                City = City!,
                PostalCode = PostalCode!,
                Country = Country!,
                PhoneNumber = PhoneNumber!,
                Email = Email!,
                Website = Website!,
                LogoUrl = LogoUrl!,
                Currency = Currency,
                CurrencySymbol = CurrencySymbol,
                DefaultTaxRate = DefaultTaxRate,
                InvoiceFooterText = InvoiceFooterText!,
                InvoicePrefix = InvoicePrefix,
                NextInvoiceNumber = NextInvoiceNumber,
                InvoiceFormat = InvoiceFormat,
                IncludeCustomerData = IncludeCustomerData,
                AllowAnonymousInvoice = AllowAnonymousInvoice,
                BusinessHours = BusinessHours!,
                IsActive = IsActive,
                CreatedByUserId = _currentUserContext.UserId
            };
        }

        private async void UploadLogo()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Arquivos de Imagem|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos os Arquivos|*.*",
                    Title = "Selecionar Logo da Empresa"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var sourceFilePath = openFileDialog.FileName;

                    // Validar se é uma imagem válida
                    if (!_fileStorageService.IsValidImage(sourceFilePath))
                    {
                        ShowStatusMessage("O arquivo selecionado não é uma imagem válida.", true);
                        return;
                    }
                    // Remover imagem anterior se existir
                    if (!string.IsNullOrEmpty(LogoUrl) && File.Exists(LogoUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(LogoUrl);
                    }
                                       
                    // Fazer upload do arquivo
                    var savedPath = await _fileStorageService.SaveLogoAsync(sourceFilePath);

                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        LogoUrl = savedPath;
                        ShowStatusMessage("Logo carregado com sucesso! Não esqueça de salvar as configurações.", false);
                    }
                    else
                    {
                        ShowStatusMessage("Erro ao salvar o logo.", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar logo: {ex.Message}", true);
            }
        }

        private async Task RemoveLogoAsync()
        {
            var result = MessageBox.Show(
                "Deseja remover o logo da empresa?",
                "Confirmar Remoção",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Deletar o arquivo físico se existir
                    if (!string.IsNullOrEmpty(LogoUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(LogoUrl);
                    }

                    // Remover do banco de dados
                    var removeResult = await _companyConfigService.RemoveLogoAsync();
                    if (removeResult.Success)
                    {
                        LogoUrl = null;
                        HasChanges = false;
                        ShowStatusMessage("Logo removido com sucesso!", false);
                    }
                    else
                    {
                        ShowStatusMessage(removeResult.Message ?? "Erro ao remover logo", true);
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Erro ao remover logo: {ex.Message}", true);
                }
            }
        }

        private async Task ActivateAsync()
        {
            try
            {
                var result = await _companyConfigService.ActivateAsync();
                if (result.Success)
                {
                    IsActive = true;
                    HasChanges = false;
                    ShowStatusMessage("Configuração ativada com sucesso!", false);
                }
                else
                {
                    ShowStatusMessage(result.Message ?? "Erro ao ativar", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao ativar: {ex.Message}", true);
            }
        }

        private async Task DeactivateAsync()
        {
            var result = MessageBox.Show(
                "Desativar a configuração pode afetar o funcionamento do sistema. Continuar?",
                "Confirmar Desativação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var deactivateResult = await _companyConfigService.DeactivateAsync();
                    if (deactivateResult.Success)
                    {
                        IsActive = false;
                        HasChanges = false;
                        ShowStatusMessage("Configuração desativada.", false);
                    }
                    else
                    {
                        ShowStatusMessage(deactivateResult.Message ?? "Erro ao desativar", true);
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Erro ao desativar: {ex.Message}", true);
                }
            }
        }

        private async Task TestInvoiceNumberAsync()
        {
            try
            {
                var result = await _companyConfigService.GenerateInvoiceNumberAsync();
                if (result.Success && result.Data != null)
                {
                    MessageBox.Show(
                        $"Próximo número de fatura que será gerado:\n\n{result.Data}",
                        "Teste de Numeração",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        result.Message ?? "Erro ao gerar número de teste",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateInvoicePreview()
        {
            InvoicePreview = $"{InvoicePrefix}-{NextInvoiceNumber:D5}";
        }

        private void ShowStatusMessage(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;

            // Reiniciar o timer para esconder a mensagem após 5 segundos
            _statusMessageTimer.Stop();
            _statusMessageTimer.Start();
        }

        #endregion

        #region Validation

        private void ValidateCompanyName()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                CompanyNameError = "O nome da empresa é obrigatório";
            }
            else if (CompanyName.Length > 200)
            {
                CompanyNameError = "O nome da empresa não pode ter mais de 200 caracteres";
            }
            else
            {
                CompanyNameError = null;
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = null;
                return;
            }

            // Simple email validation
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                EmailError = "Email inválido";
            }
            else if (Email.Length > 200)
            {
                EmailError = "O email não pode ter mais de 200 caracteres";
            }
            else
            {
                EmailError = null;
            }
        }

        private void ValidateTaxRate()
        {
            if (DefaultTaxRate < 0 || DefaultTaxRate > 100)
            {
                TaxRateError = "A taxa de imposto deve estar entre 0 e 100";
            }
            else
            {
                TaxRateError = null;
            }
        }

        private bool ValidateAll()
        {
            ValidateCompanyName();
            ValidateEmail();
            ValidateTaxRate();
            return IsValid;
        }

        #endregion
    }

    #region Helper Classes

    public class CurrencyOption
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;

        public string DisplayText => $"{Name} ({Symbol})";
    }

    public class InvoiceFormatOption
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}
