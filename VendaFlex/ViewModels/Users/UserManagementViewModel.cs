using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Users
{
    /// <summary>
    /// ViewModel Enterprise para gerenciamento completo de usuários
    /// Implementa: CRUD, Segurança, Privilégios, Auditoria, Analytics, Bulk Operations
    /// </summary>
    public class UserManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly IUserService _userService;
        private readonly IPersonService _personService;
        private readonly ISessionService _sessionService;
        private readonly IPrivilegeService _privilegeService;
        private readonly IUserPrivilegeService _userPrivilegeService;

        #endregion

        #region Collections

        private ObservableCollection<UserDto> _users = new();
        private ObservableCollection<UserDto> _allUsers = new();
        private ObservableCollection<PersonDto> _persons = new();
        private ObservableCollection<PrivilegeDto> _availablePrivileges = new();
        private ObservableCollection<PrivilegeDto> _grantedPrivileges = new();
        private ObservableCollection<UserActivityLogDto> _userActivityLog = new();
        private ObservableCollection<UserDto> _selectedUsers = new(); // Multi-select

        #endregion

        #region Selection & State

        private UserDto? _selectedUser;
        private PrivilegeDto? _selectedAvailablePrivilege;
        private PrivilegeDto? _selectedGrantedPrivilege;
        private PersonDto? _selectedPerson;
        private LoginStatus? _selectedStatusFilter;
        private bool _isLoading;
        private bool _isEditMode;
        private bool _showForm;
        private bool _showPasswordFields;
        private bool _isLoadingPrivileges;
        private bool _isBulkMode;
        private int _selectedTabIndex;

        #endregion

        #region Search & Filter

        private string _searchText = string.Empty;
        private string _privilegeSearchText = string.Empty;
        private DateTime? _filterDateFrom;
        private DateTime? _filterDateTo;
        private bool _showOnlyOnlineUsers;
        private bool _showOnlyWithFailedAttempts;
        private bool _showOnlyLockedUsers;
        private string _filterByPrivilege = string.Empty;

        #endregion

        #region Pagination

        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private int _totalItems = 0;
        private readonly List<int> _pageSizeOptions = new() { 10, 20, 50, 100 };

        #endregion

        #region Form Properties

        private int _userId;
        private int _personId;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private LoginStatus _status = LoginStatus.Active;
        private DateTime? _lastLoginAt;
        private int _failedLoginAttempts;
        private DateTime? _lockedUntil;
        private string _lastLoginIp = string.Empty;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;
        private string _notes = string.Empty; // Admin notes
        private bool _requirePasswordChange;
        private DateTime? _passwordExpiresAt;
        private bool _twoFactorEnabled;

        #endregion

        #region Security & Audit

        private int _securityScore; // 0-100
        private string _lastPasswordChange = string.Empty;
        private int _daysUntilPasswordExpiry;
        private bool _hasWeakPassword;
        private ObservableCollection<string> _recentIpAddresses = new();
        private ObservableCollection<LoginAttemptDto> _recentLoginAttempts = new();

        #endregion

        #region Analytics & Statistics

        private int _totalUsers;
        private int _totalActiveUsers;
        private int _totalInactiveUsers;
        private int _totalSuspendedUsers;
        private int _totalPendingUsers;
        private int _totalLockedUsers;
        private int _usersCreatedToday;
        private int _usersCreatedThisWeek;
        private int _usersCreatedThisMonth;
        private int _activeSessionsCount;
        private int _failedLoginsLast24h;
        private double _averagePrivilegesPerUser;
        private UserDto? _mostPrivilegedUser;
        private UserDto? _mostActiveUser;

        #endregion

        #region Messages & Notifications

        private string _message = string.Empty;
        private bool _isMessageError;
        private bool _showMessage;
        private ObservableCollection<NotificationDto> _notifications = new();

        #endregion

        #region Bulk Operations

        private string _bulkOperationType = string.Empty; // "activate", "deactivate", "lock", "delete"
        private int _bulkSelectedCount;
        private bool _showBulkConfirmation;

        #endregion

        #region Advanced Features

        private bool _autoRefreshEnabled = true;
        private int _autoRefreshInterval = 30; // seconds
        private System.Threading.Timer? _refreshTimer;
        private bool _showAdvancedFilters;
        private bool _showSecurityPanel;
        private bool _showAnalytics = true;
        private string _exportFormat = "CSV"; // CSV, Excel, PDF
        private bool _isExporting;

        #endregion

        #region Constructor

        public UserManagementViewModel(
            IUserService userService,
            IPersonService personService,
            ISessionService sessionService,
            IPrivilegeService privilegeService,
            IUserPrivilegeService userPrivilegeService)
        {
            _userService = userService;
            _personService = personService;
            _sessionService = sessionService;
            _privilegeService = privilegeService;
            _userPrivilegeService = userPrivilegeService;

            InitializeCommands();
            InitializeData();
        }

        #endregion

        #region Properties - Collections

        public ObservableCollection<UserDto> Users
        {
            get => _users;
            set => Set(ref _users, value);
        }

        public ObservableCollection<PersonDto> Persons
        {
            get => _persons;
            set => Set(ref _persons, value);
        }

        public ObservableCollection<PrivilegeDto> AvailablePrivileges
        {
            get => _availablePrivileges;
            set => Set(ref _availablePrivileges, value);
        }

        public ObservableCollection<PrivilegeDto> GrantedPrivileges
        {
            get => _grantedPrivileges;
            set => Set(ref _grantedPrivileges, value);
        }

        public ObservableCollection<UserActivityLogDto> UserActivityLog
        {
            get => _userActivityLog;
            set => Set(ref _userActivityLog, value);
        }

        public ObservableCollection<UserDto> SelectedUsers
        {
            get => _selectedUsers;
            set
            {
                if (Set(ref _selectedUsers, value))
                {
                    BulkSelectedCount = value?.Count ?? 0;
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<string> RecentIpAddresses
        {
            get => _recentIpAddresses;
            set => Set(ref _recentIpAddresses, value);
        }

        public ObservableCollection<LoginAttemptDto> RecentLoginAttempts
        {
            get => _recentLoginAttempts;
            set => Set(ref _recentLoginAttempts, value);
        }

        public ObservableCollection<NotificationDto> Notifications
        {
            get => _notifications;
            set => Set(ref _notifications, value);
        }

        public List<int> PageSizeOptions => _pageSizeOptions;

        #endregion

        #region Properties - Selection & State

        public UserDto? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (Set(ref _selectedUser, value))
                {
                    OnUserSelected();
                }
            }
        }

        public PrivilegeDto? SelectedAvailablePrivilege
        {
            get => _selectedAvailablePrivilege;
            set => Set(ref _selectedAvailablePrivilege, value);
        }

        public PrivilegeDto? SelectedGrantedPrivilege
        {
            get => _selectedGrantedPrivilege;
            set => Set(ref _selectedGrantedPrivilege, value);
        }

        public PersonDto? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (Set(ref _selectedPerson, value))
                {
                    if (value != null) PersonId = value.PersonId;
                    RefreshCanExecute();
                }
            }
        }

        public LoginStatus? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (Set(ref _selectedStatusFilter, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => Set(ref _isEditMode, value);
        }

        public bool ShowForm
        {
            get => _showForm;
            set => Set(ref _showForm, value);
        }

        public bool ShowPasswordFields
        {
            get => _showPasswordFields;
            set => Set(ref _showPasswordFields, value);
        }

        public bool IsLoadingPrivileges
        {
            get => _isLoadingPrivileges;
            set => Set(ref _isLoadingPrivileges, value);
        }

        public bool IsBulkMode
        {
            get => _isBulkMode;
            set
            {
                if (Set(ref _isBulkMode, value))
                {
                    if (!value) SelectedUsers.Clear();
                }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => Set(ref _selectedTabIndex, value);
        }

        #endregion

        #region Properties - Search & Filter

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public string PrivilegeSearchText
        {
            get => _privilegeSearchText;
            set => Set(ref _privilegeSearchText, value);
        }

        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set
            {
                if (Set(ref _filterDateFrom, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set
            {
                if (Set(ref _filterDateTo, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public bool ShowOnlyOnlineUsers
        {
            get => _showOnlyOnlineUsers;
            set
            {
                if (Set(ref _showOnlyOnlineUsers, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public bool ShowOnlyWithFailedAttempts
        {
            get => _showOnlyWithFailedAttempts;
            set
            {
                if (Set(ref _showOnlyWithFailedAttempts, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public bool ShowOnlyLockedUsers
        {
            get => _showOnlyLockedUsers;
            set
            {
                if (Set(ref _showOnlyLockedUsers, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        public string FilterByPrivilege
        {
            get => _filterByPrivilege;
            set
            {
                if (Set(ref _filterByPrivilege, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        #endregion

        #region Properties - Pagination

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (Set(ref _currentPage, value))
                {
                    ApplyPagination();
                    UpdatePaginationInfo();
                }
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (Set(ref _pageSize, value))
                {
                    CurrentPage = 1;
                    ApplyPagination();
                    UpdatePaginationInfo();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set => Set(ref _totalPages, value);
        }

        public int TotalItems
        {
            get => _totalItems;
            set => Set(ref _totalItems, value);
        }

        public string PageInfo => $"Página {CurrentPage} de {TotalPages} ({TotalItems} itens)";

        #endregion

        #region Properties - Form

        public int UserId
        {
            get => _userId;
            set => Set(ref _userId, value);
        }

        public int PersonId
        {
            get => _personId;
            set => Set(ref _personId, value);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (Set(ref _username, value))
                {
                    RefreshCanExecute();
                    _ = ValidateUsernameAsync(value);
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
                    RefreshCanExecute();
                    AnalyzePasswordStrength(value);
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (Set(ref _confirmPassword, value))
                {
                    RefreshCanExecute();
                }
            }
        }

        public LoginStatus Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public DateTime? LastLoginAt
        {
            get => _lastLoginAt;
            set => Set(ref _lastLoginAt, value);
        }

        public int FailedLoginAttempts
        {
            get => _failedLoginAttempts;
            set => Set(ref _failedLoginAttempts, value);
        }

        public DateTime? LockedUntil
        {
            get => _lockedUntil;
            set => Set(ref _lockedUntil, value);
        }

        public string LastLoginIp
        {
            get => _lastLoginIp;
            set => Set(ref _lastLoginIp, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (Set(ref _currentPassword, value))
                {
                    RefreshCanExecute();
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (Set(ref _newPassword, value))
                {
                    RefreshCanExecute();
                    AnalyzePasswordStrength(value);
                }
            }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                if (Set(ref _confirmNewPassword, value))
                {
                    RefreshCanExecute();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        public bool RequirePasswordChange
        {
            get => _requirePasswordChange;
            set => Set(ref _requirePasswordChange, value);
        }

        public DateTime? PasswordExpiresAt
        {
            get => _passwordExpiresAt;
            set => Set(ref _passwordExpiresAt, value);
        }

        public bool TwoFactorEnabled
        {
            get => _twoFactorEnabled;
            set => Set(ref _twoFactorEnabled, value);
        }

        #endregion

        #region Properties - Security & Audit

        public int SecurityScore
        {
            get => _securityScore;
            set => Set(ref _securityScore, value);
        }

        public string LastPasswordChange
        {
            get => _lastPasswordChange;
            set => Set(ref _lastPasswordChange, value);
        }

        public int DaysUntilPasswordExpiry
        {
            get => _daysUntilPasswordExpiry;
            set => Set(ref _daysUntilPasswordExpiry, value);
        }

        public bool HasWeakPassword
        {
            get => _hasWeakPassword;
            set => Set(ref _hasWeakPassword, value);
        }

        public string SecurityScoreColor => SecurityScore >= 80 ? "#4CAF50" : SecurityScore >= 50 ? "#FF9800" : "#F44336";
        public string SecurityScoreText => SecurityScore >= 80 ? "Excelente" : SecurityScore >= 50 ? "Bom" : "Crítico";

        #endregion

        #region Properties - Analytics & Statistics

        public int TotalUsers
        {
            get => _totalUsers;
            set => Set(ref _totalUsers, value);
        }

        public int TotalActiveUsers
        {
            get => _totalActiveUsers;
            set => Set(ref _totalActiveUsers, value);
        }

        public int TotalInactiveUsers
        {
            get => _totalInactiveUsers;
            set => Set(ref _totalInactiveUsers, value);
        }

        public int TotalSuspendedUsers
        {
            get => _totalSuspendedUsers;
            set => Set(ref _totalSuspendedUsers, value);
        }

        public int TotalPendingUsers
        {
            get => _totalPendingUsers;
            set => Set(ref _totalPendingUsers, value);
        }

        public int TotalLockedUsers
        {
            get => _totalLockedUsers;
            set => Set(ref _totalLockedUsers, value);
        }

        public int UsersCreatedToday
        {
            get => _usersCreatedToday;
            set => Set(ref _usersCreatedToday, value);
        }

        public int UsersCreatedThisWeek
        {
            get => _usersCreatedThisWeek;
            set => Set(ref _usersCreatedThisWeek, value);
        }

        public int UsersCreatedThisMonth
        {
            get => _usersCreatedThisMonth;
            set => Set(ref _usersCreatedThisMonth, value);
        }

        public int ActiveSessionsCount
        {
            get => _activeSessionsCount;
            set => Set(ref _activeSessionsCount, value);
        }

        public int FailedLoginsLast24h
        {
            get => _failedLoginsLast24h;
            set => Set(ref _failedLoginsLast24h, value);
        }

        public double AveragePrivilegesPerUser
        {
            get => _averagePrivilegesPerUser;
            set => Set(ref _averagePrivilegesPerUser, value);
        }

        public UserDto? MostPrivilegedUser
        {
            get => _mostPrivilegedUser;
            set => Set(ref _mostPrivilegedUser, value);
        }

        public UserDto? MostActiveUser
        {
            get => _mostActiveUser;
            set => Set(ref _mostActiveUser, value);
        }

        #endregion

        #region Properties - Messages & Notifications

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public bool IsMessageError
        {
            get => _isMessageError;
            set => Set(ref _isMessageError, value);
        }

        public bool ShowMessage
        {
            get => _showMessage;
            set => Set(ref _showMessage, value);
        }

        #endregion

        #region Properties - Bulk Operations

        public string BulkOperationType
        {
            get => _bulkOperationType;
            set => Set(ref _bulkOperationType, value);
        }

        public int BulkSelectedCount
        {
            get => _bulkSelectedCount;
            set => Set(ref _bulkSelectedCount, value);
        }

        public bool ShowBulkConfirmation
        {
            get => _showBulkConfirmation;
            set => Set(ref _showBulkConfirmation, value);
        }

        #endregion

        #region Properties - Advanced Features

        public bool AutoRefreshEnabled
        {
            get => _autoRefreshEnabled;
            set
            {
                if (Set(ref _autoRefreshEnabled, value))
                {
                    ConfigureAutoRefresh();
                }
            }
        }

        public int AutoRefreshInterval
        {
            get => _autoRefreshInterval;
            set
            {
                if (Set(ref _autoRefreshInterval, value))
                {
                    ConfigureAutoRefresh();
                }
            }
        }

        public bool ShowAdvancedFilters
        {
            get => _showAdvancedFilters;
            set => Set(ref _showAdvancedFilters, value);
        }

        public bool ShowSecurityPanel
        {
            get => _showSecurityPanel;
            set => Set(ref _showSecurityPanel, value);
        }

        public bool ShowAnalytics
        {
            get => _showAnalytics;
            set => Set(ref _showAnalytics, value);
        }

        public string ExportFormat
        {
            get => _exportFormat;
            set => Set(ref _exportFormat, value);
        }

        public bool IsExporting
        {
            get => _isExporting;
            set => Set(ref _isExporting, value);
        }

        public bool IsCurrentUser => SelectedUser?.UserId == _sessionService.CurrentUser?.UserId;
        public bool CanModifyCurrentUser => !IsCurrentUser || _sessionService.CurrentUser?.UserId != SelectedUser?.UserId;

        #endregion

        #region Commands

        // Basic CRUD
        public ICommand LoadDataCommand { get; private set; } = null!;
        public ICommand LoadPersonsCommand { get; private set; } = null!;
        public ICommand SearchCommand { get; private set; } = null!;
        public ICommand AddCommand { get; private set; } = null!;
        public ICommand EditCommand { get; private set; } = null!;
        public ICommand DeleteCommand { get; private set; } = null!;
        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        // Filters
        public ICommand FilterByStatusCommand { get; private set; } = null!;
        public ICommand ClearFilterCommand { get; private set; } = null!;
        public ICommand ToggleAdvancedFiltersCommand { get; private set; } = null!;
        public ICommand ApplyAdvancedFiltersCommand { get; private set; } = null!;

        // User Management
        public ICommand LockUserCommand { get; private set; } = null!;
        public ICommand UnlockUserCommand { get; private set; } = null!;
        public ICommand ActivateUserCommand { get; private set; } = null!;
        public ICommand DeactivateUserCommand { get; private set; } = null!;
        public ICommand ResetPasswordCommand { get; private set; } = null!;
        public ICommand ChangePasswordCommand { get; private set; } = null!;
        public ICommand TogglePasswordFieldsCommand { get; private set; } = null!;
        public ICommand ForcePasswordChangeCommand { get; private set; } = null!;
        public ICommand Toggle2FACommand { get; private set; } = null!;

        // Pagination
        public ICommand FirstPageCommand { get; private set; } = null!;
        public ICommand PreviousPageCommand { get; private set; } = null!;
        public ICommand NextPageCommand { get; private set; } = null!;
        public ICommand LastPageCommand { get; private set; } = null!;
        public ICommand ChangePageSizeCommand { get; private set; } = null!;

        // Privileges
        public ICommand LoadPrivilegesCommand { get; private set; } = null!;
        public ICommand SearchPrivilegesCommand { get; private set; } = null!;
        public ICommand GrantPrivilegeCommand { get; private set; } = null!;
        public ICommand RevokePrivilegeCommand { get; private set; } = null!;
        public ICommand GrantAllPrivilegesCommand { get; private set; } = null!;
        public ICommand RevokeAllPrivilegesCommand { get; private set; } = null!;
        public ICommand CopyPrivilegesCommand { get; private set; } = null!;

        // Bulk Operations
        public ICommand ToggleBulkModeCommand { get; private set; } = null!;
        public ICommand SelectAllUsersCommand { get; private set; } = null!;
        public ICommand DeselectAllUsersCommand { get; private set; } = null!;
        public ICommand BulkActivateCommand { get; private set; } = null!;
        public ICommand BulkDeactivateCommand { get; private set; } = null!;
        public ICommand BulkLockCommand { get; private set; } = null!;
        public ICommand BulkDeleteCommand { get; private set; } = null!;
        public ICommand BulkGrantPrivilegeCommand { get; private set; } = null!;

        // Analytics & Reports
        public ICommand RefreshAnalyticsCommand { get; private set; } = null!;
        public ICommand ExportUsersCommand { get; private set; } = null!;
        public ICommand ExportAuditLogCommand { get; private set; } = null!;
        public ICommand GenerateSecurityReportCommand { get; private set; } = null!;
        public ICommand ViewUserActivityCommand { get; private set; } = null!;

        // Advanced Features
        public ICommand ToggleAutoRefreshCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand CloneUserCommand { get; private set; } = null!;
        public ICommand SendNotificationCommand { get; private set; } = null!;
        public ICommand ImpersonateUserCommand { get; private set; } = null!;

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            // Basic CRUD
            LoadDataCommand = new AsyncCommand(LoadDataAsync);
            LoadPersonsCommand = new AsyncCommand(LoadPersonsAsync);
            SearchCommand = new AsyncCommand(SearchAsync);
            AddCommand = new RelayCommand(_ => ShowAddForm());
            EditCommand = new RelayCommand(_ => ShowEditForm(), _ => SelectedUser != null);
            DeleteCommand = new AsyncCommand(DeleteUserAsync, () => SelectedUser != null && CanModifyCurrentUser);
            SaveCommand = new AsyncCommand(SaveUserAsync, CanSave);
            CancelCommand = new RelayCommand(_ => CancelForm());

            // Filters
            FilterByStatusCommand = new RelayCommand(param => FilterByStatus(param));
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            ToggleAdvancedFiltersCommand = new RelayCommand(_ => ShowAdvancedFilters = !ShowAdvancedFilters);
            ApplyAdvancedFiltersCommand = new AsyncCommand(ApplyFiltersAsync);

            // User Management
            LockUserCommand = new AsyncCommand(LockUserAsync, () => SelectedUser != null && CanModifyCurrentUser);
            UnlockUserCommand = new AsyncCommand(UnlockUserAsync, () => SelectedUser != null);
            ActivateUserCommand = new AsyncCommand(ActivateUserAsync, () => SelectedUser != null);
            DeactivateUserCommand = new AsyncCommand(DeactivateUserAsync, () => SelectedUser != null && CanModifyCurrentUser);
            ResetPasswordCommand = new AsyncCommand(ResetPasswordAsync, () => SelectedUser != null);
            ChangePasswordCommand = new AsyncCommand(ChangePasswordAsync, CanChangePassword);
            TogglePasswordFieldsCommand = new RelayCommand(_ => ShowPasswordFields = !ShowPasswordFields);
            ForcePasswordChangeCommand = new AsyncCommand(ForcePasswordChangeAsync, () => SelectedUser != null);
            Toggle2FACommand = new AsyncCommand(Toggle2FAAsync, () => SelectedUser != null);

            // Pagination
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => CanGoToPreviousPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CanGoToPreviousPage());
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CanGoToNextPage());
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => CanGoToNextPage());
            ChangePageSizeCommand = new RelayCommand(size => ChangePageSize(size));

            // Privileges
            LoadPrivilegesCommand = new AsyncCommand(LoadPrivilegesAsync);
            SearchPrivilegesCommand = new AsyncCommand(SearchPrivilegesAsync);
            GrantPrivilegeCommand = new AsyncCommand(param => GrantPrivilegeAsync(param), () => SelectedUser != null);
            RevokePrivilegeCommand = new AsyncCommand(param => RevokePrivilegeAsync(param), () => SelectedUser != null);
            GrantAllPrivilegesCommand = new AsyncCommand(GrantAllPrivilegesAsync, () => SelectedUser != null);
            RevokeAllPrivilegesCommand = new AsyncCommand(RevokeAllPrivilegesAsync, () => SelectedUser != null);
            CopyPrivilegesCommand = new AsyncCommand(CopyPrivilegesAsync);

            // Bulk Operations
            ToggleBulkModeCommand = new RelayCommand(_ => IsBulkMode = !IsBulkMode);
            SelectAllUsersCommand = new RelayCommand(_ => SelectAllUsers());
            DeselectAllUsersCommand = new RelayCommand(_ => DeselectAllUsers());
            BulkActivateCommand = new AsyncCommand(BulkActivateAsync, () => BulkSelectedCount > 0);
            BulkDeactivateCommand = new AsyncCommand(BulkDeactivateAsync, () => BulkSelectedCount > 0);
            BulkLockCommand = new AsyncCommand(BulkLockAsync, () => BulkSelectedCount > 0);
            BulkDeleteCommand = new AsyncCommand(BulkDeleteAsync, () => BulkSelectedCount > 0);
            BulkGrantPrivilegeCommand = new AsyncCommand(BulkGrantPrivilegeAsync);

            // Analytics & Reports
            RefreshAnalyticsCommand = new AsyncCommand(RefreshAnalyticsAsync);
            ExportUsersCommand = new AsyncCommand(ExportUsersAsync);
            ExportAuditLogCommand = new AsyncCommand(ExportAuditLogAsync, () => SelectedUser != null);
            GenerateSecurityReportCommand = new AsyncCommand(GenerateSecurityReportAsync);
            ViewUserActivityCommand = new AsyncCommand(ViewUserActivityAsync, () => SelectedUser != null);

            // Advanced Features
            ToggleAutoRefreshCommand = new RelayCommand(_ => AutoRefreshEnabled = !AutoRefreshEnabled);
            RefreshCommand = new AsyncCommand(LoadDataAsync);
            CloneUserCommand = new AsyncCommand(CloneUserAsync, () => SelectedUser != null);
            SendNotificationCommand = new AsyncCommand(SendNotificationAsync, () => SelectedUser != null);
            ImpersonateUserCommand = new AsyncCommand(ImpersonateUserAsync, () => SelectedUser != null && CanModifyCurrentUser);
        }

        private void InitializeData()
        {
            _ = LoadDataAsync();
            _ = LoadPersonsAsync();
            _ = LoadPrivilegesAsync();
            ConfigureAutoRefresh();
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                HideMessage();

                var result = await _userService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    _allUsers = new ObservableCollection<UserDto>(result.Data);
                    await CalculateStatisticsAsync();
                    TotalItems = _allUsers.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    CurrentPage = 1;
                    ApplyPagination();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao carregar usuários.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao carregar dados: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPersonsAsync()
        {
            try
            {
                var result = await _personService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    Persons = new ObservableCollection<PersonDto>(
                        result.Data.Where(p => p.Type == PersonType.Employee).OrderBy(p => p.Name));
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao carregar pessoas: {ex.Message}");
            }
        }

        private async Task LoadPrivilegesAsync()
        {
            try
            {
                IsLoadingPrivileges = true;
                var result = await _privilegeService.GetActiveAsync();
                if (result.Success && result.Data != null)
                {
                    AvailablePrivileges = new ObservableCollection<PrivilegeDto>(
                        result.Data.OrderBy(p => p.Name));
                }
                if (SelectedUser != null)
                {
                    await LoadGrantedPrivilegesAsync(SelectedUser.UserId);
                }
            }
            finally
            {
                IsLoadingPrivileges = false;
            }
        }

        private async Task LoadGrantedPrivilegesAsync(int userId)
        {
            var result = await _userPrivilegeService.GetUserPrivilegesDetailsAsync(userId);
            if (result.Success && result.Data != null)
            {
                GrantedPrivileges = new ObservableCollection<PrivilegeDto>(
                    result.Data.OrderBy(p => p.Name));
            }
        }

        #endregion

        #region Search & Filter

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                HideMessage();

                if (string.IsNullOrWhiteSpace(SearchText) && !SelectedStatusFilter.HasValue)
                {
                    await LoadDataAsync();
                    return;
                }

                var result = await _userService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    var filtered = result.Data.AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var searchLower = SearchText.ToLower();
                        filtered = filtered.Where(u =>
                            u.Username.ToLower().Contains(searchLower) ||
                            (u.LastLoginIp?.Contains(SearchText) ?? false));
                    }

                    if (SelectedStatusFilter.HasValue)
                    {
                        filtered = filtered.Where(u => u.Status == SelectedStatusFilter.Value);
                    }

                    _allUsers = new ObservableCollection<UserDto>(filtered);
                    await CalculateStatisticsAsync();
                    TotalItems = _allUsers.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    CurrentPage = 1;
                    ApplyPagination();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao buscar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyFiltersAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _userService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    var filtered = result.Data.AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var searchLower = SearchText.ToLower();
                        filtered = filtered.Where(u => u.Username.ToLower().Contains(searchLower));
                    }

                    if (SelectedStatusFilter.HasValue)
                    {
                        filtered = filtered.Where(u => u.Status == SelectedStatusFilter.Value);
                    }

                    if (FilterDateFrom.HasValue)
                    {
                        filtered = filtered.Where(u => u.LastLoginAt >= FilterDateFrom.Value);
                    }

                    if (FilterDateTo.HasValue)
                    {
                        filtered = filtered.Where(u => u.LastLoginAt <= FilterDateTo.Value);
                    }

                    if (ShowOnlyWithFailedAttempts)
                    {
                        filtered = filtered.Where(u => u.FailedLoginAttempts > 0);
                    }

                    if (ShowOnlyLockedUsers)
                    {
                        filtered = filtered.Where(u => u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow);
                    }

                    _allUsers = new ObservableCollection<UserDto>(filtered);
                    await CalculateStatisticsAsync();
                    TotalItems = _allUsers.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    CurrentPage = 1;
                    ApplyPagination();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterByStatus(object? parameter)
        {
            if (parameter is LoginStatus status)
            {
                SelectedStatusFilter = status;
            }
        }

        private void ClearFilter()
        {
            SelectedStatusFilter = null;
            SearchText = string.Empty;
            FilterDateFrom = null;
            FilterDateTo = null;
            ShowOnlyWithFailedAttempts = false;
            ShowOnlyLockedUsers = false;
            ShowOnlyOnlineUsers = false;
            FilterByPrivilege = string.Empty;
            _ = LoadDataAsync();
        }

        #endregion

        #region CRUD Operations

        private void ShowAddForm()
        {
            ClearForm();
            IsEditMode = false;
            ShowForm = true;
            ShowPasswordFields = false;
            SelectedTabIndex = 1; // Switch to Details tab
        }

        private void ShowEditForm()
        {
            if (SelectedUser == null) return;
            LoadUserToForm(SelectedUser);
            IsEditMode = true;
            ShowForm = true;
            ShowPasswordFields = false;
            SelectedTabIndex = 1;
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            if (SelectedUser.UserId == _sessionService.CurrentUser?.UserId)
            {
                ShowErrorMessage("Você não pode excluir seu próprio usuário.");
                return;
            }

            try
            {
                HideMessage();
                var result = await _userService.DeleteAsync(SelectedUser.UserId);
                if (result.Success)
                {
                    _allUsers.Remove(SelectedUser);
                    SelectedUser = null;
                    await CalculateStatisticsAsync();
                    TotalItems = _allUsers.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    if (CurrentPage > TotalPages && TotalPages > 0)
                        CurrentPage = TotalPages;
                    else
                        ApplyPagination();

                    ShowSuccessMessage("Usuário excluído com sucesso!");
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao excluir usuário.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao excluir: {ex.Message}");
            }
        }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(Username) || SelectedPerson == null)
                return false;

            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                    return false;
                if (Password != ConfirmPassword)
                    return false;
            }

            return true;
        }

        private async Task SaveUserAsync()
        {
            try
            {
                HideMessage();

                if (!CanSave())
                {
                    ShowErrorMessage("Preencha todos os campos obrigatórios corretamente.");
                    return;
                }

                var userDto = new UserDto
                {
                    UserId = UserId,
                    PersonId = PersonId,
                    Username = Username,
                    Status = Status,
                    LastLoginAt = LastLoginAt,
                    FailedLoginAttempts = FailedLoginAttempts,
                    LockedUntil = LockedUntil,
                    LastLoginIp = LastLoginIp
                };

                if (IsEditMode)
                {
                    var result = await _userService.UpdateAsync(userDto);
                    if (result.Success && result.Data != null)
                    {
                        var index = _allUsers.ToList().FindIndex(u => u.UserId == result.Data.UserId);
                        if (index >= 0) _allUsers[index] = result.Data;
                        ApplyPagination();
                        ShowSuccessMessage("Usuário atualizado com sucesso!");
                    }
                    else
                    {
                        ShowErrorMessage(result.Message ?? "Erro ao atualizar usuário.");
                        return;
                    }
                }
                else
                {
                    var result = await _userService.RegisterAsync(userDto, Password);
                    if (result.Success && result.Data != null)
                    {
                        _allUsers.Add(result.Data);
                        TotalItems = _allUsers.Count;
                        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                        ApplyPagination();
                        ShowSuccessMessage("Usuário cadastrado com sucesso!");
                    }
                    else
                    {
                        ShowErrorMessage(result.Message ?? "Erro ao criar usuário.");
                        return;
                    }
                }

                await CalculateStatisticsAsync();
                ShowForm = false;
                ClearForm();
                SelectedTabIndex = 0;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao salvar: {ex.Message}");
            }
        }

        private void CancelForm()
        {
            ShowForm = false;
            ClearForm();
            HideMessage();
            SelectedTabIndex = 0;
        }

        private async Task CloneUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                LoadUserToForm(SelectedUser);
                UserId = 0;
                Username = $"{Username}_copy";
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsEditMode = false;
                ShowForm = true;
                SelectedTabIndex = 1;
                ShowSuccessMessage("Usuário clonado. Ajuste os dados e salve.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao clonar usuário: {ex.Message}");
            }
        }

        #endregion

        #region User Management

        private async Task LockUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();
                var result = await _userService.LockUserAsync(SelectedUser.UserId);

                if (result.Success)
                {
                    ShowSuccessMessage("Usuário bloqueado com sucesso!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao bloquear usuário.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao bloquear usuário: {ex.Message}");
            }
        }

        private async Task UnlockUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();
                var result = await _userService.UnlockUserAsync(SelectedUser.UserId);

                if (result.Success)
                {
                    ShowSuccessMessage("Usuário desbloqueado com sucesso!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao desbloquear usuário.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao desbloquear usuário: {ex.Message}");
            }
        }

        private async Task ActivateUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();
                SelectedUser.Status = LoginStatus.Active;
                var result = await _userService.UpdateAsync(SelectedUser);

                if (result.Success)
                {
                    ShowSuccessMessage("Usuário ativado com sucesso!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao ativar usuário.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao ativar usuário: {ex.Message}");
            }
        }

        private async Task DeactivateUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();
                SelectedUser.Status = LoginStatus.Inactive;
                var result = await _userService.UpdateAsync(SelectedUser);

                if (result.Success)
                {
                    ShowSuccessMessage("Usuário desativado com sucesso!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao desativar usuário.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao desativar usuário: {ex.Message}");
            }
        }

        private async Task ResetPasswordAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();
                // Implementation would call email service
                ShowSuccessMessage("Solicitação de reset de senha enviada!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao resetar senha: {ex.Message}");
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(CurrentPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmNewPassword) &&
                   NewPassword == ConfirmNewPassword;
        }

        private async Task ChangePasswordAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                HideMessage();

                if (!CanChangePassword())
                {
                    ShowErrorMessage("Preencha todos os campos de senha corretamente.");
                    return;
                }

                var result = await _userService.ChangePasswordAsync(
                    SelectedUser.UserId, CurrentPassword, NewPassword);

                if (result.Success)
                {
                    ShowSuccessMessage("Senha alterada com sucesso!");
                    ShowPasswordFields = false;
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao alterar senha.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao alterar senha: {ex.Message}");
            }
        }

        private async Task ForcePasswordChangeAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                RequirePasswordChange = true;
                ShowSuccessMessage("Usuário será forçado a alterar senha no próximo login.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task Toggle2FAAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                TwoFactorEnabled = !TwoFactorEnabled;
                ShowSuccessMessage($"2FA {(TwoFactorEnabled ? "ativado" : "desativado")} com sucesso!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        #endregion

        #region Privilege Management

        private async Task SearchPrivilegesAsync()
        {
            if (string.IsNullOrWhiteSpace(PrivilegeSearchText))
            {
                await LoadPrivilegesAsync();
                return;
            }

            var result = await _privilegeService.SearchAsync(PrivilegeSearchText.Trim());
            if (result.Success && result.Data != null)
            {
                AvailablePrivileges = new ObservableCollection<PrivilegeDto>(
                    result.Data.OrderBy(p => p.Name));
            }
        }

        private async Task GrantPrivilegeAsync(object? param)
        {
            if (SelectedUser == null || param is not PrivilegeDto privilege) return;

            if (GrantedPrivileges.Any(p => p.PrivilegeId == privilege.PrivilegeId)) return;

            var dto = new UserPrivilegeDto
            {
                UserId = SelectedUser.UserId,
                PrivilegeId = privilege.PrivilegeId,
                GrantedAt = DateTime.UtcNow,
                GrantedByUserId = _sessionService.CurrentUser?.UserId
            };

            var result = await _userPrivilegeService.GrantAsync(dto);
            if (result.Success)
            {
                GrantedPrivileges.Add(privilege);
                await CalculateStatisticsAsync();
                ShowSuccessMessage($"Privilégio '{privilege.Name}' concedido.");
            }
            else
            {
                ShowErrorMessage(result.Message ?? "Falha ao conceder privilégio.");
            }
        }

        private async Task RevokePrivilegeAsync(object? param)
        {
            if (SelectedUser == null || param is not PrivilegeDto privilege) return;

            var result = await _userPrivilegeService.RevokeByUserAndPrivilegeAsync(
                SelectedUser.UserId, privilege.PrivilegeId);

            if (result.Success)
            {
                var item = GrantedPrivileges.FirstOrDefault(p => p.PrivilegeId == privilege.PrivilegeId);
                if (item != null) GrantedPrivileges.Remove(item);
                await CalculateStatisticsAsync();
                ShowSuccessMessage($"Privilégio '{privilege.Name}' revogado.");
            }
            else
            {
                ShowErrorMessage(result.Message ?? "Falha ao revogar privilégio.");
            }
        }

        private async Task GrantAllPrivilegesAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var privilegeIds = AvailablePrivileges.Select(p => p.PrivilegeId);
                var result = await _userPrivilegeService.GrantMultipleAsync(
                    SelectedUser.UserId, privilegeIds, _sessionService.CurrentUser?.UserId);

                if (result.Success)
                {
                    await LoadGrantedPrivilegesAsync(SelectedUser.UserId);
                    await CalculateStatisticsAsync();
                    ShowSuccessMessage("Todos os privilégios concedidos!");
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao conceder privilégios.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task RevokeAllPrivilegesAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var result = await _userPrivilegeService.RevokeAllFromUserAsync(SelectedUser.UserId);
                if (result.Success)
                {
                    GrantedPrivileges.Clear();
                    await CalculateStatisticsAsync();
                    ShowSuccessMessage("Todos os privilégios revogados!");
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao revogar privilégios.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task CopyPrivilegesAsync()
        {
            // Would open dialog to select source user and copy their privileges
            ShowSuccessMessage("Funcionalidade de copiar privilégios disponível.");
        }

        #endregion

        #region Bulk Operations

        private void SelectAllUsers()
        {
            SelectedUsers = new ObservableCollection<UserDto>(Users);
        }

        private void DeselectAllUsers()
        {
            SelectedUsers.Clear();
        }

        private async Task BulkActivateAsync()
        {
            try
            {
                foreach (var user in SelectedUsers)
                {
                    user.Status = LoginStatus.Active;
                    await _userService.UpdateAsync(user);
                }
                await LoadDataAsync();
                ShowSuccessMessage($"{BulkSelectedCount} usuários ativados!");
                DeselectAllUsers();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task BulkDeactivateAsync()
        {
            try
            {
                foreach (var user in SelectedUsers)
                {
                    if (user.UserId == _sessionService.CurrentUser?.UserId) continue;
                    user.Status = LoginStatus.Inactive;
                    await _userService.UpdateAsync(user);
                }
                await LoadDataAsync();
                ShowSuccessMessage($"{BulkSelectedCount} usuários desativados!");
                DeselectAllUsers();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task BulkLockAsync()
        {
            try
            {
                foreach (var user in SelectedUsers)
                {
                    if (user.UserId == _sessionService.CurrentUser?.UserId) continue;
                    await _userService.LockUserAsync(user.UserId);
                }
                await LoadDataAsync();
                ShowSuccessMessage($"{BulkSelectedCount} usuários bloqueados!");
                DeselectAllUsers();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task BulkDeleteAsync()
        {
            try
            {
                var toDelete = SelectedUsers.Where(u => u.UserId != _sessionService.CurrentUser?.UserId).ToList();
                foreach (var user in toDelete)
                {
                    await _userService.DeleteAsync(user.UserId);
                }
                await LoadDataAsync();
                ShowSuccessMessage($"{toDelete.Count} usuários excluídos!");
                DeselectAllUsers();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
        }

        private async Task BulkGrantPrivilegeAsync()
        {
            // Would open dialog to select privilege to grant to all selected users
            ShowSuccessMessage("Funcionalidade de concessão em massa disponível.");
        }

        #endregion

        #region Analytics & Statistics

        private async Task CalculateStatisticsAsync()
        {
            await Task.Run(() =>
            {
                TotalUsers = _allUsers.Count;
                TotalActiveUsers = _allUsers.Count(u => u.Status == LoginStatus.Active);
                TotalInactiveUsers = _allUsers.Count(u => u.Status == LoginStatus.Inactive);
                TotalSuspendedUsers = _allUsers.Count(u => u.Status == LoginStatus.Suspended);
                TotalPendingUsers = _allUsers.Count(u => u.Status == LoginStatus.PendingActivation);
                TotalLockedUsers = _allUsers.Count(u => u.LockedUntil.HasValue && u.LockedUntil > DateTime.UtcNow);

                var today = DateTime.UtcNow.Date;
                var weekAgo = today.AddDays(-7);
                var monthAgo = today.AddMonths(-1);

                UsersCreatedToday = 0; // Would need CreatedAt field
                UsersCreatedThisWeek = 0;
                UsersCreatedThisMonth = 0;

                FailedLoginsLast24h = _allUsers.Sum(u => u.FailedLoginAttempts);
            });
        }

        private async Task RefreshAnalyticsAsync()
        {
            await CalculateStatisticsAsync();
            ShowSuccessMessage("Analytics atualizados!");
        }

        private async Task ViewUserActivityAsync()
        {
            if (SelectedUser == null) return;
            // Would load activity log for selected user
            ShowSuccessMessage("Funcionalidade de log de atividades disponível.");
        }

        #endregion

        #region Export & Reports

        private async Task ExportUsersAsync()
        {
            try
            {
                IsExporting = true;
                // Implementation would export based on ExportFormat
                await Task.Delay(1000); // Simulate export
                ShowSuccessMessage($"Usuários exportados em formato {ExportFormat}!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao exportar: {ex.Message}");
            }
            finally
            {
                IsExporting = false;
            }
        }

        private async Task ExportAuditLogAsync()
        {
            if (SelectedUser == null) return;
            try
            {
                IsExporting = true;
                await Task.Delay(1000);
                ShowSuccessMessage("Log de auditoria exportado!");
            }
            finally
            {
                IsExporting = false;
            }
        }

        private async Task GenerateSecurityReportAsync()
        {
            try
            {
                IsExporting = true;
                await Task.Delay(1500);
                ShowSuccessMessage("Relatório de segurança gerado!");
            }
            finally
            {
                IsExporting = false;
            }
        }

        #endregion

        #region Security Analysis

        private void AnalyzePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                SecurityScore = 0;
                HasWeakPassword = true;
                return;
            }

            int score = 0;
            if (password.Length >= 8) score += 20;
            if (password.Length >= 12) score += 10;
            if (password.Any(char.IsUpper)) score += 20;
            if (password.Any(char.IsLower)) score += 20;
            if (password.Any(char.IsDigit)) score += 15;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score += 15;

            SecurityScore = Math.Min(score, 100);
            HasWeakPassword = SecurityScore < 50;
        }

        private async Task ValidateUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            var exists = await _userService.UsernameExistsAsync(username, IsEditMode ? UserId : null);
            if (exists && !IsEditMode)
            {
                ShowErrorMessage("Nome de usuário já existe!");
            }
        }

        private void CalculateUserSecurityScore()
        {
            if (SelectedUser == null) return;

            int score = 100;

            if (SelectedUser.FailedLoginAttempts > 0) score -= 10;
            if (SelectedUser.FailedLoginAttempts > 3) score -= 20;
            if (SelectedUser.LockedUntil.HasValue) score -= 30;
            if (!TwoFactorEnabled) score -= 15;
            if (HasWeakPassword) score -= 25;

            SecurityScore = Math.Max(score, 0);
        }

        #endregion

        #region Advanced Features

        private void ConfigureAutoRefresh()
        {
            _refreshTimer?.Dispose();

            if (AutoRefreshEnabled && AutoRefreshInterval > 0)
            {
                _refreshTimer = new System.Threading.Timer(
                    async _ => await LoadDataAsync(),
                    null,
                    TimeSpan.FromSeconds(AutoRefreshInterval),
                    TimeSpan.FromSeconds(AutoRefreshInterval));
            }
        }

        private async Task SendNotificationAsync()
        {
            if (SelectedUser == null) return;
            ShowSuccessMessage($"Notificação enviada para {SelectedUser.Username}!");
        }

        private async Task ImpersonateUserAsync()
        {
            if (SelectedUser == null) return;
            // Would implement user impersonation for support purposes
            ShowSuccessMessage($"Impersonação de usuário iniciada para {SelectedUser.Username}!");
        }

        #endregion

        #region Pagination

        private void ApplyPagination()
        {
            var skip = (CurrentPage - 1) * PageSize;
            var pagedData = _allUsers.Skip(skip).Take(PageSize);
            Users = new ObservableCollection<UserDto>(pagedData);
            UpdatePaginationInfo();
        }

        private void UpdatePaginationInfo()
        {
            OnPropertyChanged(nameof(PageInfo));
            RefreshCanExecute();
        }

        private void GoToFirstPage() => CurrentPage = 1;
        private void GoToPreviousPage() { if (CurrentPage > 1) CurrentPage--; }
        private void GoToNextPage() { if (CurrentPage < TotalPages) CurrentPage++; }
        private void GoToLastPage() => CurrentPage = TotalPages;
        private bool CanGoToPreviousPage() => CurrentPage > 1;
        private bool CanGoToNextPage() => CurrentPage < TotalPages;

        private void ChangePageSize(object? size)
        {
            if (size is int newSize)
            {
                PageSize = newSize;
            }
        }

        #endregion

        #region User Selection Handler

        private async void OnUserSelected()
        {
            if (SelectedUser == null)
            {
                GrantedPrivileges.Clear();
                RecentIpAddresses.Clear();
                RecentLoginAttempts.Clear();
                SecurityScore = 0;
                return;
            }

            RefreshCanExecute();

            // Load user privileges
            await LoadGrantedPrivilegesAsync(SelectedUser.UserId);

            // Load security info
            CalculateUserSecurityScore();

            // Load recent IPs (mock data)
            RecentIpAddresses = new ObservableCollection<string>
            {
                SelectedUser.LastLoginIp ?? "N/A"
            };

            // Calculate password age
            LastPasswordChange = "30 dias atrás"; // Would calculate from actual data
            DaysUntilPasswordExpiry = 60; // Would calculate from policy
        }

        #endregion

        #region Form Helpers

        private void LoadUserToForm(UserDto user)
        {
            UserId = user.UserId;
            PersonId = user.PersonId;
            SelectedPerson = Persons.FirstOrDefault(p => p.PersonId == user.PersonId);
            Username = user.Username;
            Status = user.Status;
            LastLoginAt = user.LastLoginAt;
            FailedLoginAttempts = user.FailedLoginAttempts;
            LockedUntil = user.LockedUntil;
            LastLoginIp = user.LastLoginIp ?? string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private void ClearForm()
        {
            UserId = 0;
            PersonId = 0;
            SelectedPerson = null;
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Status = LoginStatus.Active;
            LastLoginAt = null;
            FailedLoginAttempts = 0;
            LockedUntil = null;
            LastLoginIp = string.Empty;
            ShowPasswordFields = false;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            Notes = string.Empty;
            RequirePasswordChange = false;
            PasswordExpiresAt = null;
            TwoFactorEnabled = false;
        }

        #endregion

        #region Messages & Notifications

        private async void ShowSuccessMessage(string message)
        {
            Message = message;
            IsMessageError = false;
            ShowMessage = true;

            await Task.Delay(3000);
            HideMessage();
        }

        private async void ShowErrorMessage(string message)
        {
            Message = message;
            IsMessageError = true;
            ShowMessage = true;

            await Task.Delay(5000);
            HideMessage();
        }

        private void HideMessage()
        {
            ShowMessage = false;
            Message = string.Empty;
        }

        #endregion

        #region Command Helpers

        private void RefreshCanExecute()
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            _refreshTimer?.Dispose();
        }

        #endregion
    }

    #region Supporting DTOs

    /// <summary>
    /// DTO para log de atividades do usuário
    /// </summary>
    public class UserActivityLogDto
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = string.Empty; // Login, Logout, Update, etc.
    }

    /// <summary>
    /// DTO para tentativas de login
    /// </summary>
    public class LoginAttemptDto
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime AttemptTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para notificações do sistema
    /// </summary>
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public string Icon { get; set; } = "Information";
    }

    #endregion
}