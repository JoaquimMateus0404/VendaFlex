using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Authentication
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private readonly ICredentialManager _credentialManager;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _rememberMe;
        private bool _showPassword;

        public LoginViewModel(
            IUserService userService, 
            ISessionService sessionService,
            INavigationService navigationService,
            ICredentialManager credentialManager)
        {
            _userService = userService;
            _sessionService = sessionService;
            _navigationService = navigationService;
            _credentialManager = credentialManager;

            LoginCommand = new AsyncCommand(ExecuteLoginAsync, CanLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(_ => ShowPassword = !ShowPassword);
            ForgotPasswordCommand = new RelayCommand(_ => ExecuteForgotPassword());
            
            // Carregar credenciais salvas ao inicializar
            LoadSavedCredentials();
        }

        public string Username
        {
            get => _username;
            set
            {
                if (Set(ref _username, value))
                {
                    ClearError();
                    ((AsyncCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (Set(ref _password, value))
                {
                    ClearError();
                    ((AsyncCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => Set(ref _rememberMe, value);
        }

        public bool ShowPassword
        {
            get => _showPassword;
            set => Set(ref _showPassword, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username)
                   && !string.IsNullOrWhiteSpace(Password)
                   && !IsLoading;
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                // Simular delay de rede para mostrar a animação
                await Task.Delay(800);

                var result = await _userService.LoginAsync(Username, Password);

                if (!result.Success || result.Data == null)
                {
                    ErrorMessage = result.Message ?? "Usuário ou senha incorretos.";
                    return;
                }

                var user = result.Data;

                // Verificar status do usuário
                if (user.Status == LoginStatus.Inactive) // Inactive
                {
                    ErrorMessage = "Conta inativa. Contacte o administrador.";
                    return;
                }

                if (user.Status == LoginStatus.Suspended) // Suspended
                {
                    ErrorMessage = "Conta suspensa. Contacte o administrador.";
                    return;
                }

                if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                {
                    var remainingTime = user.LockedUntil.Value - DateTime.UtcNow;
                    ErrorMessage = $"Conta bloqueada. Tente novamente em {(int)remainingTime.TotalMinutes} minutos.";
                    return;
                }

                // Login bem-sucedido - Iniciar sessão
                _sessionService.StartSession(user, GetLocalIpAddress());

                // Salvar ou remover credenciais baseado em "Lembrar-me"
                if (RememberMe)
                {
                    SaveCredentials(Username);
                }
                else
                {
                    ClearSavedCredentials();
                }

                // Navegar para a tela principal (Dashboard)
                try
                {
                    _navigationService.NavigateToDashBoard();
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro na navegação para o DashBoard: {navEx}");
                    ErrorMessage = "Login efetuado, mas houve um erro ao abrir o painel. Tente novamente.";
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao fazer login: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteForgotPassword()
        {
            MessageBox.Show("Funcionalidade de recuperação de senha em desenvolvimento.",
                "Esqueceu a senha", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        private void SaveCredentials(string username)
        {
            try
            {
                var success = _credentialManager.SaveRememberedUsername(username);
                if (!success)
                {
                    // Log error silently - não interromper o fluxo de login
                    System.Diagnostics.Debug.WriteLine("Falha ao salvar credenciais no Credential Manager");
                }
            }
            catch (Exception ex)
            {
                // Log error silently
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar credenciais: {ex.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                _credentialManager.ClearRememberedUsername();
            }
            catch (Exception ex)
            {
                // Log error silently
                System.Diagnostics.Debug.WriteLine($"Erro ao limpar credenciais: {ex.Message}");
            }
        }

        public void LoadSavedCredentials()
        {
            try
            {
                var savedUsername = _credentialManager.GetRememberedUsername();
                if (!string.IsNullOrEmpty(savedUsername))
                {
                    Username = savedUsername;
                    RememberMe = true;
                }
            }
            catch (Exception ex)
            {
                // Log error silently
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar credenciais: {ex.Message}");
            }
        }

        private string? GetLocalIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var ipAddress = host.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return ipAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
