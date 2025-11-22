using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;
using Microsoft.Win32;
using System.IO;

namespace VendaFlex.ViewModels.Settings
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly IPersonService _personService;
        
        private UserDto? _currentUser;
        private PersonDto? _currentPerson;
        private string _name = string.Empty;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _mobileNumber = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = string.Empty;
        private string _profileImageUrl = string.Empty;
        private string _taxId = string.Empty;
        private string _identificationNumber = string.Empty;
        
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        
        private bool _isLoading;
        private bool _isSaving;
        private bool _isEditMode;
        private bool _showPasswordSection;
        private string _successMessage = string.Empty;
        private string _errorMessage = string.Empty;

        public UserProfileViewModel(
            ISessionService sessionService,
            IUserService userService,
            IPersonService personService)
        {
            _sessionService = sessionService;
            _userService = userService;
            _personService = personService;

            EditCommand = new RelayCommand(_ => IsEditMode = true, _ => !IsEditMode);
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditMode);
            SaveCommand = new AsyncCommand(SaveChangesAsync, () => IsEditMode && !IsSaving);
            ChangePasswordCommand = new AsyncCommand(ChangePasswordAsync, CanChangePassword);
            UploadPhotoCommand = new AsyncCommand(UploadPhotoAsync);
            TogglePasswordSectionCommand = new RelayCommand(_ => ShowPasswordSection = !ShowPasswordSection);
            
            LoadUserProfile();
        }

        #region Properties

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
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

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => Set(ref _profileImageUrl, value);
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

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (Set(ref _currentPassword, value))
                    ((AsyncCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (Set(ref _newPassword, value))
                    ((AsyncCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (Set(ref _confirmPassword, value))
                    ((AsyncCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (Set(ref _isSaving, value))
                    ((AsyncCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (Set(ref _isEditMode, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool ShowPasswordSection
        {
            get => _showPasswordSection;
            set => Set(ref _showPasswordSection, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => Set(ref _successMessage, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public DateTime? LastLoginAt => _currentUser?.LastLoginAt;
        public string LastLoginIp => _currentUser?.LastLoginIp ?? "N/A";
        public string StatusDisplay => _currentUser?.Status.ToString() ?? "Desconhecido";

        #endregion

        #region Commands

        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        public ICommand TogglePasswordSectionCommand { get; }

        #endregion

        #region Methods

        private async void LoadUserProfile()
        {
            try
            {
                IsLoading = true;
                ClearMessages();

                _currentUser = _sessionService.CurrentUser;
                if (_currentUser == null)
                {
                    ErrorMessage = "Nenhum usuário logado.";
                    return;
                }

                // Carregar dados do usuário
                Username = _currentUser.Username;
                
                // Carregar dados da pessoa
                var personResult = await _personService.GetByIdAsync(_currentUser.PersonId);
                if (personResult.Success && personResult.Data != null)
                {
                    _currentPerson = personResult.Data;
                    Name = _currentPerson.Name;
                    Email = _currentPerson.Email ?? string.Empty;
                    PhoneNumber = _currentPerson.PhoneNumber ?? string.Empty;
                    MobileNumber = _currentPerson.MobileNumber ?? string.Empty;
                    Address = _currentPerson.Address ?? string.Empty;
                    City = _currentPerson.City ?? string.Empty;
                    State = _currentPerson.State ?? string.Empty;
                    PostalCode = _currentPerson.PostalCode ?? string.Empty;
                    Country = _currentPerson.Country ?? string.Empty;
                    ProfileImageUrl = _currentPerson.ProfileImageUrl ?? string.Empty;
                    TaxId = _currentPerson.TaxId ?? string.Empty;
                    IdentificationNumber = _currentPerson.IdentificationNumber ?? string.Empty;
                }

                OnPropertyChanged(nameof(LastLoginAt));
                OnPropertyChanged(nameof(LastLoginIp));
                OnPropertyChanged(nameof(StatusDisplay));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar perfil: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            LoadUserProfile();
            IsEditMode = false;
            ClearMessages();
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                IsSaving = true;
                ClearMessages();

                if (_currentUser == null)
                {
                    ErrorMessage = "Nenhum usuário logado.";
                    return;
                }

                // Validações básicas
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ErrorMessage = "O nome é obrigatório.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "O nome de usuário é obrigatório.";
                    return;
                }

                if (_currentPerson == null)
                {
                    ErrorMessage = "Dados da pessoa não encontrados.";
                    return;
                }

                // Atualizar dados do usuário (username)
                var updateUserDto = new UserDto
                {
                    UserId = _currentUser.UserId,
                    PersonId = _currentUser.PersonId,
                    Username = Username,
                    PasswordHash = _currentUser.PasswordHash,
                    Status = _currentUser.Status,
                    LastLoginAt = _currentUser.LastLoginAt,
                    FailedLoginAttempts = _currentUser.FailedLoginAttempts,
                    LockedUntil = _currentUser.LockedUntil,
                    LastLoginIp = _currentUser.LastLoginIp
                };

                var userResult = await _userService.UpdateAsync(updateUserDto);
                
                // Atualizar dados da pessoa
                var updatePersonDto = new PersonDto
                {
                    PersonId = _currentPerson.PersonId,
                    Name = Name,
                    Type = _currentPerson.Type,
                    TaxId = TaxId,
                    IdentificationNumber = IdentificationNumber,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    MobileNumber = MobileNumber,
                    Website = _currentPerson.Website,
                    Address = Address,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    CreditLimit = _currentPerson.CreditLimit,
                    CurrentBalance = _currentPerson.CurrentBalance,
                    ContactPerson = _currentPerson.ContactPerson,
                    Notes = _currentPerson.Notes,
                    ProfileImageUrl = ProfileImageUrl,
                    IsActive = _currentPerson.IsActive,
                    Rating = _currentPerson.Rating
                };

                var personResult = await _personService.UpdateAsync(updatePersonDto);
                
                if (userResult.Success && personResult.Success)
                {
                    // Atualizar dados locais
                    _currentUser = updateUserDto;
                    _currentPerson = updatePersonDto;
                    
                    SuccessMessage = "Perfil atualizado com sucesso!";
                    IsEditMode = false;
                    
                    // Limpar mensagem de sucesso após 3 segundos
                    await Task.Delay(3000);
                    SuccessMessage = string.Empty;
                }
                else
                {
                    var errors = string.Join(", ", userResult.Errors.Concat(personResult.Errors));
                    ErrorMessage = $"Erro ao atualizar perfil: {errors}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar alterações: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(CurrentPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   NewPassword == ConfirmPassword &&
                   NewPassword.Length >= 6;
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                ClearMessages();

                if (_currentUser == null)
                {
                    ErrorMessage = "Nenhum usuário logado.";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "As senhas não coincidem.";
                    return;
                }

                if (NewPassword.Length < 6)
                {
                    ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.";
                    return;
                }

                // Tentar alterar senha
                var result = await _userService.ChangePasswordAsync(_currentUser.UserId, CurrentPassword, NewPassword);
                
                if (result.Success)
                {
                    SuccessMessage = "Senha alterada com sucesso!";
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                    ShowPasswordSection = false;
                    
                    // Limpar mensagem de sucesso após 3 segundos
                    await Task.Delay(3000);
                    SuccessMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Senha atual incorreta ou erro ao alterar senha.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao alterar senha: {ex.Message}";
            }
        }

        private async Task UploadPhotoAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                    Title = "Selecionar foto de perfil"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;
                    
                    // TODO: Implementar upload de arquivo para servidor ou pasta local
                    // Por enquanto, vamos apenas salvar o caminho
                    ProfileImageUrl = filePath;
                    
                    SuccessMessage = "Foto carregada! Não esqueça de salvar as alterações.";
                    await Task.Delay(3000);
                    SuccessMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar foto: {ex.Message}";
            }
        }

        private void ClearMessages()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        #endregion
    }
}
