using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Persons
{
    public class PrivilegeSelectionItem : BaseViewModel
    {
        private bool _isSelected;

        public int PrivilegeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
    }

    public class PersonFormDialogViewModel : BaseViewModel
    {
        private readonly IPersonService _personService;
        private readonly IUserService _userService;
        private readonly IPrivilegeService _privilegeService;
        private readonly IUserPrivilegeService _userPrivilegeService;
        private readonly IFileStorageService _fileStorageService;
        private readonly Window _dialogWindow;

        private const string DefaultPassword = "VendaFlex@123";

        private int _currentStep = 1;
        private bool _isLoading;
        private bool _isEditMode;
        private int? _personId;
        private int? _userId;

        // Step 1 - Dados Pessoais
        private string _name = string.Empty;
        private PersonType _type = PersonType.Customer;
        private string _taxId = string.Empty;
        private string _identificationNumber = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _mobileNumber = string.Empty;
        private string _website = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = string.Empty;
        private decimal _creditLimit;
        private decimal _currentBalance;
        private string _contactPerson = string.Empty;
        private string _notes = string.Empty;
        private string _profileImageUrl = string.Empty;
        private bool _isActive = true;
        private int? _rating;

        // Step 2 - Usuário
        private string _generatedUsername = string.Empty;
        private LoginStatus _userStatus = LoginStatus.Active;

        // Step 3 - Privilégios
        private ObservableCollection<PrivilegeSelectionItem> _availablePrivileges = new();
        private ObservableCollection<PrivilegeSelectionItem> _allPrivileges = new();
        private string _privilegeSearchText = string.Empty;

        public PersonFormDialogViewModel(
            IPersonService personService,
            IUserService userService,
            IPrivilegeService privilegeService,
            IUserPrivilegeService userPrivilegeService,
            IFileStorageService fileStorageService,
            Window dialogWindow,
            PersonDto? personToEdit = null)
        {
            _personService = personService;
            _userService = userService;
            _privilegeService = privilegeService;
            _userPrivilegeService = userPrivilegeService;
            _fileStorageService = fileStorageService;
            _dialogWindow = dialogWindow;

            // Comandos
            NextStepCommand = new AsyncCommand(NextStepAsync, CanGoNext);
            PreviousStepCommand = new RelayCommand(_ => PreviousStep(), _ => CanGoPrevious());
            CancelCommand = new RelayCommand(_ => Cancel());
            UploadPhotoCommand = new AsyncCommand(UploadPhotoAsync);
            RemovePhotoCommand = new RelayCommand(_ => RemovePhoto());
            SearchPrivilegesCommand = new RelayCommand(_ => SearchPrivileges());
            SelectAllPrivilegesCommand = new RelayCommand(_ => SelectAllPrivileges());
            DeselectAllPrivilegesCommand = new RelayCommand(_ => DeselectAllPrivileges());

            // Carregar dados se for edição
            if (personToEdit != null)
            {
                _isEditMode = true;
                _personId = personToEdit.PersonId;
                LoadPersonData(personToEdit);
                
                // Notificar que o comando pode ser executado
                ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
            }

            // Em modo de edição, não precisa carregar privilégios (não vai mostrar steps de usuário)
            // Em modo criação, carregar privilégios apenas se for funcionário
            if (!_isEditMode)
            {
                _ = InitializeAsync();
            }
            
            // Forçar reavaliação inicial do comando após construção
            Task.Run(async () =>
            {
                await Task.Delay(100); // Pequeno delay para garantir que a UI está pronta
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
                });
            });
        }

        #region Properties

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (Set(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(ShowStep1));
                    OnPropertyChanged(nameof(ShowStep2));
                    OnPropertyChanged(nameof(ShowStep3));
                    OnPropertyChanged(nameof(ShowPreviousButton));
                    OnPropertyChanged(nameof(NextButtonText));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool IsEmployee => Type == PersonType.Employee;

        public bool ShowStep1 => CurrentStep == 1;
        // Em modo de edição, nunca mostra steps 2 e 3 (apenas dados pessoais podem ser editados)
        public bool ShowStep2 => CurrentStep == 2 && IsEmployee && !_isEditMode;
        public bool ShowStep3 => CurrentStep == 3 && IsEmployee && !_isEditMode;
        public bool ShowPreviousButton => CurrentStep > 1 && IsEmployee && !_isEditMode;

        public string NextButtonText
        {
            get
            {
                // Em modo de edição, sempre "SALVAR"
                if (_isEditMode) return "SALVAR";
                
                // Em modo criação
                return IsEmployee
                    ? (CurrentStep == 3 ? "FINALIZAR" : "PRÓXIMO")
                    : "SALVAR";
            }
        }

        public string WindowTitle => _isEditMode ? "Editar Pessoa" : "Nova Pessoa";
        public string WindowSubtitle => _isEditMode 
            ? "Atualize as informações da pessoa" 
            : IsEmployee 
                ? "Complete as 3 etapas para criar um novo funcionário"
                : "Preencha os dados para criar uma nova pessoa";

        // Step 1
        public string Name
        {
            get => _name;
            set
            {
                if (Set(ref _name, value))
                {
                    GenerateUsername();
                    ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public PersonType Type
        {
            get => _type;
            set
            {
                if (Set(ref _type, value))
                {
                    OnPropertyChanged(nameof(IsEmployee));
                    OnPropertyChanged(nameof(IsCustomer));
                    OnPropertyChanged(nameof(IsSupplier));
                    OnPropertyChanged(nameof(WindowSubtitle));
                    OnPropertyChanged(nameof(NextButtonText));
                    ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
                    if (value == PersonType.Employee && _allPrivileges.Count == 0)
                    {
                        _ = LoadPrivilegesAsync();
                    }
                }
            }
        }

        public string TaxId
        {
            get => _taxId;
            set => Set(ref _taxId, value);
        }

        public string IdentificationNumber
        {
            get => _identificationNumber;
            set => Set(ref _identificationNumber, value);
        }

        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => Set(ref _phoneNumber, value);
        }

        public string MobileNumber
        {
            get => _mobileNumber;
            set => Set(ref _mobileNumber, value);
        }

        public string Website
        {
            get => _website;
            set => Set(ref _website, value);
        }

        public string Address
        {
            get => _address;
            set => Set(ref _address, value);
        }

        public string City
        {
            get => _city;
            set => Set(ref _city, value);
        }

        public string State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public string PostalCode
        {
            get => _postalCode;
            set => Set(ref _postalCode, value);
        }

        public string Country
        {
            get => _country;
            set => Set(ref _country, value);
        }

        public decimal CreditLimit
        {
            get => _creditLimit;
            set => Set(ref _creditLimit, value);
        }

        public decimal CurrentBalance
        {
            get => _currentBalance;
            set => Set(ref _currentBalance, value);
        }

        public string ContactPerson
        {
            get => _contactPerson;
            set => Set(ref _contactPerson, value);
        }

        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => Set(ref _profileImageUrl, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        public int? Rating
        {
            get => _rating;
            set => Set(ref _rating, value);
        }

        public bool IsCustomer => Type == PersonType.Customer || Type == PersonType.Both;
        public bool IsSupplier => Type == PersonType.Supplier || Type == PersonType.Both;

        // Step 2
        public string GeneratedUsername
        {
            get => _generatedUsername;
            set
            {
                if (Set(ref _generatedUsername, value))
                {
                    ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public LoginStatus UserStatus
        {
            get => _userStatus;
            set => Set(ref _userStatus, value);
        }

        // Step 3
        public ObservableCollection<PrivilegeSelectionItem> AvailablePrivileges
        {
            get => _availablePrivileges;
            set => Set(ref _availablePrivileges, value);
        }

        public string PrivilegeSearchText
        {
            get => _privilegeSearchText;
            set
            {
                if (Set(ref _privilegeSearchText, value))
                {
                    SearchPrivileges();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand NextStepCommand { get; }
        public ICommand PreviousStepCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        public ICommand RemovePhotoCommand { get; }
        public ICommand SearchPrivilegesCommand { get; }
        public ICommand SelectAllPrivilegesCommand { get; }
        public ICommand DeselectAllPrivilegesCommand { get; }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            if (IsEmployee)
            {
                await LoadPrivilegesAsync();
            }
        }

        private void LoadPersonData(PersonDto person)
        {
            Name = person.Name;
            Type = person.Type;
            TaxId = person.TaxId ?? string.Empty;
            IdentificationNumber = person.IdentificationNumber ?? string.Empty;
            Email = person.Email ?? string.Empty;
            PhoneNumber = person.PhoneNumber ?? string.Empty;
            MobileNumber = person.MobileNumber ?? string.Empty;
            Website = person.Website ?? string.Empty;
            Address = person.Address ?? string.Empty;
            City = person.City ?? string.Empty;
            State = person.State ?? string.Empty;
            PostalCode = person.PostalCode ?? string.Empty;
            Country = person.Country ?? string.Empty;
            CreditLimit = person.CreditLimit;
            CurrentBalance = person.CurrentBalance;
            ContactPerson = person.ContactPerson ?? string.Empty;
            Notes = person.Notes ?? string.Empty;
            ProfileImageUrl = person.ProfileImageUrl ?? string.Empty;
            IsActive = person.IsActive;
            Rating = person.Rating;
        }

        private void GenerateUsername()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                GeneratedUsername = string.Empty;
                return;
            }

            // Remover acentos e caracteres especiais
            var normalized = RemoveDiacritics(Name.ToLower());

            // Pegar primeiro nome e sobrenome
            var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                GeneratedUsername = string.Empty;
                return;
            }

            if (parts.Length == 1)
            {
                GeneratedUsername = parts[0];
            }
            else
            {
                // Primeiro nome + primeira letra do último sobrenome
                var firstName = parts[0];
                var lastNameInitial = parts[^1][0];
                GeneratedUsername = $"{firstName}.{lastNameInitial}";
            }

            // Remover caracteres inválidos
            GeneratedUsername = new string(GeneratedUsername
                .Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_')
                .ToArray());
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private async Task LoadPrivilegesAsync()
        {
            try
            {
                IsLoading = true;

                var result = await _privilegeService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    _allPrivileges.Clear();
                    foreach (var privilege in result.Data.Where(p => p.IsActive))
                    {
                        _allPrivileges.Add(new PrivilegeSelectionItem
                        {
                            PrivilegeId = privilege.PrivilegeId,
                            Name = privilege.Name,
                            Description = privilege.Description ?? string.Empty,
                            Code = privilege.Code ?? string.Empty,
                            IsSelected = false
                        });
                    }

                    AvailablePrivileges = new ObservableCollection<PrivilegeSelectionItem>(_allPrivileges);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar privilégios: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SearchPrivileges()
        {
            if (string.IsNullOrWhiteSpace(PrivilegeSearchText))
            {
                AvailablePrivileges = new ObservableCollection<PrivilegeSelectionItem>(_allPrivileges);
                return;
            }

            var searchTerm = PrivilegeSearchText.ToLower();
            var filtered = _allPrivileges.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Code.ToLower().Contains(searchTerm)
            ).ToList();

            AvailablePrivileges = new ObservableCollection<PrivilegeSelectionItem>(filtered);
        }

        private void SelectAllPrivileges()
        {
            foreach (var privilege in AvailablePrivileges)
            {
                privilege.IsSelected = true;
            }
        }

        private void DeselectAllPrivileges()
        {
            foreach (var privilege in AvailablePrivileges)
            {
                privilege.IsSelected = false;
            }
        }

        private async Task UploadPhotoAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Imagens|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                    Title = "Selecionar Foto"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    
                    if (_fileStorageService.IsValidImage(dialog.FileName))
                    {
                        var savedPath = await _fileStorageService.SaveLogoAsync(dialog.FileName);
                        ProfileImageUrl = savedPath;
                    }
                    else
                    {
                        MessageBox.Show("Por favor, selecione uma imagem válida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fazer upload da foto: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RemovePhoto()
        {
            try
            {
                var result = MessageBox.Show(
                    "Tem certeza que deseja remover a foto de perfil?",
                    "Confirmar Remoção",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Tentar deletar o arquivo se existir
                    if (!string.IsNullOrWhiteSpace(ProfileImageUrl))
                    {
                        try
                        {
                            _ = _fileStorageService.DeleteFileAsync(ProfileImageUrl);
                        }
                        catch
                        {
                            // Ignorar erro ao deletar arquivo
                        }
                    }

                    ProfileImageUrl = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao remover foto: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanGoNext()
        {
            if (IsLoading) return false;

            switch (CurrentStep)
            {
                case 1:
                    return !string.IsNullOrWhiteSpace(Name);
                case 2:
                    return !string.IsNullOrWhiteSpace(GeneratedUsername);
                case 3:
                    return true; // Privilégios são opcionais
                default:
                    return false;
            }
        }

        private bool CanGoPrevious()
        {
            return CurrentStep > 1 && !IsLoading;
        }

        private async Task NextStepAsync()
        {
            // Em modo de edição, sempre salvar direto (não navega pelos steps)
            if (_isEditMode)
            {
                await SaveAsync();
                return;
            }

            // Se não for funcionário, salvar direto
            if (!IsEmployee)
            {
                await SaveAsync();
                return;
            }

            // Se for funcionário (modo criação), navegar pelos steps
            if (CurrentStep < 3)
            {
                CurrentStep++;
                ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
            }
            else
            {
                // Step 3 - Finalizar
                await SaveAsync();
            }
        }

        private void PreviousStep()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
                ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                IsLoading = true;
                // 1. Salvar Person
                var personDto = new PersonDto
                {
                    PersonId = _personId ?? 0,
                    Name = Name,
                    Type = Type,
                    TaxId = TaxId,
                    IdentificationNumber = IdentificationNumber,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    MobileNumber = MobileNumber,
                    Website = Website,
                    Address = Address,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    CreditLimit = CreditLimit,
                    CurrentBalance = CurrentBalance,
                    ContactPerson = ContactPerson,
                    Notes = Notes,
                    ProfileImageUrl = ProfileImageUrl,
                    IsActive = IsActive,
                    Rating = Rating
                };

                var personResult = _isEditMode
                    ? await _personService.UpdateAsync(personDto)
                    : await _personService.CreateAsync(personDto);

                if (!personResult.Success)
                {
                    MessageBox.Show(personResult.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var savedPersonId = personResult.Data?.PersonId ?? _personId ?? 0;

                // 2. Se for funcionário E modo criação, criar usuário e privilégios
                // Em modo de edição, NÃO mexe em usuário/privilégios
                if (IsEmployee && !_isEditMode)
                {
                    // Criar novo usuário
                    var userDto = new UserDto
                    {
                        PersonId = savedPersonId,
                        Username = GeneratedUsername,
                        Status = UserStatus
                    };

                    var userResult = await _userService.RegisterAsync(userDto, DefaultPassword);
                    if (!userResult.Success)
                    {
                        MessageBox.Show(userResult.Message, "Erro ao criar usuário", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _userId = userResult.Data?.UserId;

                    // 3. Salvar privilégios
                    if (_userId.HasValue)
                    {
                        var selectedPrivileges = _allPrivileges.Where(p => p.IsSelected).ToList();

                        // Adicionar privilégios selecionados
                        foreach (var privilege in selectedPrivileges)
                        {
                            var userPrivDto = new UserPrivilegeDto
                            {
                                UserId = _userId.Value,
                                PrivilegeId = privilege.PrivilegeId
                            };

                            await _userPrivilegeService.GrantAsync(userPrivDto);
                        }
                    }
                }

                MessageBox.Show(
                    _isEditMode ? "Pessoa atualizada com sucesso!" : "Pessoa criada com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _dialogWindow.DialogResult = true;
                _dialogWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            _dialogWindow.DialogResult = false;
            _dialogWindow.Close();
        }

        #endregion
    }
}

