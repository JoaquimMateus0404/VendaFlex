using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Settings
{
    /// <summary>
    /// ViewModel para gerenciamento completo de despesas.
    /// </summary>
    public class ExpenseManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly IExpenseService _expenseService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly DispatcherTimer _statusMessageTimer;

        #endregion

        #region Constructor

        public ExpenseManagementViewModel(
            IExpenseService expenseService,
            ICurrentUserContext currentUserContext)
        {
            _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));

            // Initialize timer
            _statusMessageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _statusMessageTimer.Tick += (s, e) =>
            {
                StatusMessage = null;
                _statusMessageTimer.Stop();
            };

            InitializeCommands();
            InitializeCollections();
            _ = LoadDataAsync();
        }

        #endregion

        #region Collections

        private ObservableCollection<ExpenseDto> _expenses = new();
        public ObservableCollection<ExpenseDto> Expenses
        {
            get => _expenses;
            set => Set(ref _expenses, value);
        }

        private ObservableCollection<ExpenseDto> _filteredExpenses = new();
        public ObservableCollection<ExpenseDto> FilteredExpenses
        {
            get => _filteredExpenses;
            set => Set(ref _filteredExpenses, value);
        }

        private ObservableCollection<ExpenseTypeDto> _expenseTypes = new();
        public ObservableCollection<ExpenseTypeDto> ExpenseTypes
        {
            get => _expenseTypes;
            set => Set(ref _expenseTypes, value);
        }

        private ObservableCollection<ExpenseDto> _unpaidExpenses = new();
        public ObservableCollection<ExpenseDto> UnpaidExpenses
        {
            get => _unpaidExpenses;
            set => Set(ref _unpaidExpenses, value);
        }

        #endregion

        #region Selected Items

        private ExpenseDto? _selectedExpense;
        public ExpenseDto? SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                if (Set(ref _selectedExpense, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        #endregion

        #region Form Properties

        private int _expenseId;
        public int ExpenseId
        {
            get => _expenseId;
            set => Set(ref _expenseId, value);
        }

        private int _expenseTypeId;
        public int ExpenseTypeId
        {
            get => _expenseTypeId;
            set => Set(ref _expenseTypeId, value);
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set => Set(ref _date, value);
        }

        private decimal _value;
        public decimal Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        private string _reference = string.Empty;
        public string Reference
        {
            get => _reference;
            set => Set(ref _reference, value);
        }

        private string _attachmentUrl = string.Empty;
        public string AttachmentUrl
        {
            get => _attachmentUrl;
            set => Set(ref _attachmentUrl, value);
        }

        private bool _isPaid;
        public bool IsPaid
        {
            get => _isPaid;
            set => Set(ref _isPaid, value);
        }

        private DateTime? _paidDate;
        public DateTime? PaidDate
        {
            get => _paidDate;
            set => Set(ref _paidDate, value);
        }

        #endregion

        #region Filter Properties

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Set(ref _searchText, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        private int? _filterExpenseTypeId;
        public int? FilterExpenseTypeId
        {
            get => _filterExpenseTypeId;
            set
            {
                if (Set(ref _filterExpenseTypeId, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        private bool _filterUnpaidOnly;
        public bool FilterUnpaidOnly
        {
            get => _filterUnpaidOnly;
            set
            {
                if (Set(ref _filterUnpaidOnly, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        private DateTime? _filterStartDate;
        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set
            {
                if (Set(ref _filterStartDate, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        private DateTime? _filterEndDate;
        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set
            {
                if (Set(ref _filterEndDate, value))
                {
                    _ = ApplyFiltersAsync();
                }
            }
        }

        #endregion

        #region Statistics

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set => Set(ref _totalAmount, value);
        }

        private decimal _totalPaidAmount;
        public decimal TotalPaidAmount
        {
            get => _totalPaidAmount;
            set => Set(ref _totalPaidAmount, value);
        }

        private decimal _totalUnpaidAmount;
        public decimal TotalUnpaidAmount
        {
            get => _totalUnpaidAmount;
            set => Set(ref _totalUnpaidAmount, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => Set(ref _totalCount, value);
        }

        #endregion

        #region UI State

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private bool _isFormOpen;
        public bool IsFormOpen
        {
            get => _isFormOpen;
            set => Set(ref _isFormOpen, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => Set(ref _isEditing, value);
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

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; private set; } = null!;
        public ICommand AddExpenseCommand { get; private set; } = null!;
        public ICommand EditExpenseCommand { get; private set; } = null!;
        public ICommand DeleteExpenseCommand { get; private set; } = null!;
        public ICommand SaveExpenseCommand { get; private set; } = null!;
        public ICommand CancelEditCommand { get; private set; } = null!;
        public ICommand MarkAsPaidCommand { get; private set; } = null!;
        public ICommand MarkAsUnpaidCommand { get; private set; } = null!;
        public ICommand ClearFiltersCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddExpenseCommand = new RelayCommand(_ => OpenAddForm());
            EditExpenseCommand = new RelayCommand(_ => OpenEditForm(), _ => SelectedExpense != null);
            DeleteExpenseCommand = new RelayCommand(async _ => await DeleteExpenseAsync(), _ => SelectedExpense != null);
            SaveExpenseCommand = new RelayCommand(async _ => await SaveExpenseAsync());
            CancelEditCommand = new RelayCommand(_ => CloseForm());
            MarkAsPaidCommand = new RelayCommand(async _ => await MarkAsPaidAsync(), _ => SelectedExpense != null && !SelectedExpense.IsPaid);
            MarkAsUnpaidCommand = new RelayCommand(async _ => await MarkAsUnpaidAsync(), _ => SelectedExpense != null && SelectedExpense.IsPaid);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        }

        #endregion

        #region Initialization

        private void InitializeCollections()
        {
            Expenses = new ObservableCollection<ExpenseDto>();
            FilteredExpenses = new ObservableCollection<ExpenseDto>();
            ExpenseTypes = new ObservableCollection<ExpenseTypeDto>();
            UnpaidExpenses = new ObservableCollection<ExpenseDto>();
        }

        #endregion

        #region Data Loading

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await LoadExpensesAsync();
                await LoadExpenseTypesAsync();
                await LoadUnpaidExpensesAsync();
                await LoadStatisticsAsync();

                ShowSuccessMessage("Dados carregados com sucesso!");
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

        private async Task LoadExpensesAsync()
        {
            var result = await _expenseService.GetAllAsync();
            if (result.Success && result.Data != null)
            {
                Expenses.Clear();
                foreach (var expense in result.Data)
                {
                    Expenses.Add(expense);
                }
                await ApplyFiltersAsync();
            }
        }

        private async Task LoadExpenseTypesAsync()
        {
            var result = await _expenseService.GetActiveExpenseTypesAsync();
            if (result.Success && result.Data != null)
            {
                ExpenseTypes.Clear();
                foreach (var type in result.Data)
                {
                    ExpenseTypes.Add(type);
                }
            }
        }

        private async Task LoadUnpaidExpensesAsync()
        {
            var result = await _expenseService.GetUnpaidExpensesAsync();
            if (result.Success && result.Data != null)
            {
                UnpaidExpenses.Clear();
                foreach (var expense in result.Data)
                {
                    UnpaidExpenses.Add(expense);
                }
            }
        }

        private async Task LoadStatisticsAsync()
        {
            var totalResult = await _expenseService.GetTotalAmountAsync();
            if (totalResult.Success)
                TotalAmount = totalResult.Data;

            var paidResult = await _expenseService.GetTotalPaidAmountAsync();
            if (paidResult.Success)
                TotalPaidAmount = paidResult.Data;

            var unpaidResult = await _expenseService.GetTotalUnpaidAmountAsync();
            if (unpaidResult.Success)
                TotalUnpaidAmount = unpaidResult.Data;

            var countResult = await _expenseService.GetTotalCountAsync();
            if (countResult.Success)
                TotalCount = countResult.Data;
        }

        #endregion

        #region Filtering

        private async Task ApplyFiltersAsync()
        {
            await Task.Run(() =>
            {
                var filtered = Expenses.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filtered = filtered.Where(e =>
                        (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(e.Notes) && e.Notes.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(e.Reference) && e.Reference.ToLower().Contains(search)));
                }

                if (FilterExpenseTypeId.HasValue && FilterExpenseTypeId.Value > 0)
                {
                    filtered = filtered.Where(e => e.ExpenseTypeId == FilterExpenseTypeId.Value);
                }

                if (FilterUnpaidOnly)
                {
                    filtered = filtered.Where(e => !e.IsPaid);
                }

                if (FilterStartDate.HasValue)
                {
                    filtered = filtered.Where(e => e.Date >= FilterStartDate.Value);
                }

                if (FilterEndDate.HasValue)
                {
                    filtered = filtered.Where(e => e.Date <= FilterEndDate.Value);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FilteredExpenses.Clear();
                    foreach (var expense in filtered.OrderByDescending(e => e.Date))
                    {
                        FilteredExpenses.Add(expense);
                    }
                });
            });
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            FilterExpenseTypeId = null;
            FilterUnpaidOnly = false;
            FilterStartDate = null;
            FilterEndDate = null;
        }

        #endregion

        #region CRUD Operations

        private void OpenAddForm()
        {
            IsEditing = false;
            ClearForm();
            IsFormOpen = true;
        }

        private void OpenEditForm()
        {
            if (SelectedExpense == null) return;

            IsEditing = true;
            LoadExpenseToForm(SelectedExpense);
            IsFormOpen = true;
        }

        private void CloseForm()
        {
            IsFormOpen = false;
            ClearForm();
        }

        private void ClearForm()
        {
            ExpenseId = 0;
            ExpenseTypeId = 0;
            Date = DateTime.Now;
            Value = 0;
            Title = string.Empty;
            Notes = string.Empty;
            Reference = string.Empty;
            AttachmentUrl = string.Empty;
            IsPaid = false;
            PaidDate = null;
        }

        private void LoadExpenseToForm(ExpenseDto expense)
        {
            ExpenseId = expense.ExpenseId;
            ExpenseTypeId = expense.ExpenseTypeId;
            Date = expense.Date;
            Value = expense.Value;
            Title = expense.Title ?? string.Empty;
            Notes = expense.Notes ?? string.Empty;
            Reference = expense.Reference ?? string.Empty;
            AttachmentUrl = expense.AttachmentUrl ?? string.Empty;
            IsPaid = expense.IsPaid;
            PaidDate = expense.PaidDate;
        }

        private async Task SaveExpenseAsync()
        {
            if (!ValidateForm()) return;

            IsLoading = true;
            try
            {
                var dto = new ExpenseDto
                {
                    ExpenseId = ExpenseId,
                    ExpenseTypeId = ExpenseTypeId,
                    UserId = _currentUserContext.UserId ?? 0,
                    Date = Date,
                    Value = Value,
                    Title = Title,
                    Notes = Notes,
                    Reference = Reference,
                    AttachmentUrl = AttachmentUrl,
                    IsPaid = IsPaid,
                    PaidDate = PaidDate
                };

                if (IsEditing)
                {
                    var result = await _expenseService.UpdateAsync(dto);
                    if (result.Success)
                    {
                        ShowSuccessMessage("Despesa atualizada com sucesso!");
                        CloseForm();
                        await LoadDataAsync();
                    }
                    else
                    {
                        ShowErrorMessage($"Erro: {result.Message}");
                    }
                }
                else
                {
                    var result = await _expenseService.CreateAsync(dto);
                    if (result.Success)
                    {
                        ShowSuccessMessage("Despesa criada com sucesso!");
                        CloseForm();
                        await LoadDataAsync();
                    }
                    else
                    {
                        ShowErrorMessage($"Erro: {result.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao salvar: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteExpenseAsync()
        {
            if (SelectedExpense == null) return;

            var result = MessageBox.Show(
                $"Excluir despesa '{SelectedExpense.Title}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                var deleteResult = await _expenseService.DeleteAsync(SelectedExpense.ExpenseId);
                if (deleteResult.Success)
                {
                    ShowSuccessMessage("Despesa excluída!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage($"Erro: {deleteResult.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao excluir: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Payment Operations

        private async Task MarkAsPaidAsync()
        {
            if (SelectedExpense == null || SelectedExpense.IsPaid) return;

            IsLoading = true;
            try
            {
                var result = await _expenseService.MarkAsPaidAsync(SelectedExpense.ExpenseId, DateTime.Now);
                if (result.Success)
                {
                    ShowSuccessMessage("Marcada como paga!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage($"Erro: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task MarkAsUnpaidAsync()
        {
            if (SelectedExpense == null || !SelectedExpense.IsPaid) return;

            IsLoading = true;
            try
            {
                var result = await _expenseService.MarkAsUnpaidAsync(SelectedExpense.ExpenseId);
                if (result.Success)
                {
                    ShowSuccessMessage("Marcada como não paga!");
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorMessage($"Erro: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Validation

        private bool ValidateForm()
        {
            if (ExpenseTypeId <= 0)
            {
                ShowErrorMessage("Selecione um tipo de despesa.");
                return false;
            }

            if (Value <= 0)
            {
                ShowErrorMessage("O valor deve ser maior que zero.");
                return false;
            }

            if (Date > DateTime.Now)
            {
                ShowErrorMessage("A data não pode ser futura.");
                return false;
            }

            if (IsPaid && !PaidDate.HasValue)
            {
                ShowErrorMessage("Informe a data de pagamento.");
                return false;
            }

            return true;
        }

        #endregion

        #region Helper Methods

        private void ShowSuccessMessage(string message)
        {
            StatusMessage = message;
            IsStatusError = false;
            _statusMessageTimer.Stop();
            _statusMessageTimer.Start();
        }

        private void ShowErrorMessage(string message)
        {
            StatusMessage = message;
            IsStatusError = true;
            _statusMessageTimer.Stop();
            _statusMessageTimer.Start();
        }

        #endregion
    }
}

