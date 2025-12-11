using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Persons
{
    public class PersonManagementViewModel : BaseViewModel
    {
        private readonly IPersonService _personService;
        private readonly IUserService _userService;
        private readonly IPrivilegeService _privilegeService;
        private readonly IUserPrivilegeService _userPrivilegeService;
        private readonly IFileStorageService _fileStorageService;
        private ObservableCollection<PersonDto> _persons;
        private ObservableCollection<PersonDto> _allPersons;
        private PersonDto? _selectedPerson;
        private string _searchText = string.Empty;
        private PersonType? _selectedTypeFilter;
        private bool _isLoading;
        private string _message = string.Empty;
        private bool _isMessageError;
        private bool _showMessage;
        
        // Paginação
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private int _totalItems = 0;


        public PersonManagementViewModel(
            IPersonService personService, 
            IUserService userService,
            IPrivilegeService privilegeService,
            IUserPrivilegeService userPrivilegeService,
            IFileStorageService fileStorageService)
        {
            _personService = personService;
            _userService = userService;
            _privilegeService = privilegeService;
            _userPrivilegeService = userPrivilegeService;
            _fileStorageService = fileStorageService;
            _persons = new ObservableCollection<PersonDto>();
            _allPersons = new ObservableCollection<PersonDto>();

            // Comandos
            LoadDataCommand = new AsyncCommand(LoadDataAsync);
            SearchCommand = new AsyncCommand(SearchAsync);
            AddCommand = new RelayCommand(_ => ShowAddForm());
            EditCommand = new RelayCommand(_ => ShowEditForm(), _ => SelectedPerson != null);
            DeleteCommand = new AsyncCommand(DeletePersonAsync, () => SelectedPerson != null);
            FilterByTypeCommand = new RelayCommand(param => FilterByType(param));
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            
            // Comandos de paginação
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => CanGoToPreviousPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CanGoToPreviousPage());
            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CanGoToNextPage());
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => CanGoToNextPage());

            // Carregar dados iniciais
            _ = LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<PersonDto> Persons
        {
            get => _persons;
            set => Set(ref _persons, value);
        }

        public PersonDto? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (Set(ref _selectedPerson, value))
                {
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => Set(ref _searchText, value);
        }

        public PersonType? SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set => Set(ref _selectedTypeFilter, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }


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

        // Propriedades de paginação
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


        public int TotalPersons => _allPersons.Count;
        public int TotalCustomers => _allPersons.Count(p => p.Type == PersonType.Customer || p.Type == PersonType.Both);
        public int TotalSuppliers => _allPersons.Count(p => p.Type == PersonType.Supplier || p.Type == PersonType.Both);
        public int TotalEmployees => _allPersons.Count(p => p.Type == PersonType.Employee);

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand FilterByTypeCommand { get; }
        public ICommand ClearFilterCommand { get; }
        
        // Comandos de paginação
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                HideMessage();

                var result = await _personService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    _allPersons = new ObservableCollection<PersonDto>(result.Data);
                    TotalItems = _allPersons.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    CurrentPage = 1;
                    ApplyPagination();
                    UpdateCounters();
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao carregar pessoas.");
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

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                HideMessage();

                if (string.IsNullOrWhiteSpace(SearchText) && !SelectedTypeFilter.HasValue)
                {
                    await LoadDataAsync();
                    return;
                }

                var result = await _personService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    var filtered = result.Data.AsEnumerable();

                    // Filtro por texto
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var searchLower = SearchText.ToLower();
                        filtered = filtered.Where(p =>
                            p.Name.ToLower().Contains(searchLower) ||
                            (p.Email?.ToLower().Contains(searchLower) ?? false) ||
                            (p.TaxId?.ToLower().Contains(searchLower) ?? false) ||
                            (p.PhoneNumber?.Contains(SearchText) ?? false) ||
                            (p.MobileNumber?.Contains(SearchText) ?? false));
                    }

                    // Filtro por tipo
                    if (SelectedTypeFilter.HasValue)
                    {
                        filtered = filtered.Where(p => p.Type == SelectedTypeFilter.Value || 
                                                      (SelectedTypeFilter.Value == PersonType.Customer && p.Type == PersonType.Both) ||
                                                      (SelectedTypeFilter.Value == PersonType.Supplier && p.Type == PersonType.Both));
                    }

                    _allPersons = new ObservableCollection<PersonDto>(filtered);
                    TotalItems = _allPersons.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    CurrentPage = 1;
                    ApplyPagination();
                    UpdateCounters();
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

        private void ShowAddForm()
        {
            var dialog = new UI.Views.Persons.PersonFormDialog();
            var viewModel = new PersonFormDialogViewModel(
                _personService,
                _userService,
                _privilegeService,
                _userPrivilegeService,
                _fileStorageService,
                dialog);
            
            dialog.DataContext = viewModel;
            
            if (dialog.ShowDialog() == true)
            {
                // Recarregar dados após criação
                _ = LoadDataAsync();
            }
        }

        private void ShowEditForm()
        {
            if (SelectedPerson == null) return;

            var dialog = new UI.Views.Persons.PersonFormDialog();
            var viewModel = new PersonFormDialogViewModel(
                _personService,
                _userService,
                _privilegeService,
                _userPrivilegeService,
                _fileStorageService,
                dialog,
                SelectedPerson);
            
            dialog.DataContext = viewModel;
            
            if (dialog.ShowDialog() == true)
            {
                // Recarregar dados após edição
                _ = LoadDataAsync();
            }
        }

        private async Task DeletePersonAsync()
        {
            if (SelectedPerson == null) return;

            try
            {
                HideMessage();

                var result = await _personService.DeleteAsync(SelectedPerson.PersonId);
                if (result.Success)
                {
                    _allPersons.Remove(SelectedPerson);
                    SelectedPerson = null;
                    TotalItems = _allPersons.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                    
                    if (CurrentPage > TotalPages && TotalPages > 0)
                        CurrentPage = TotalPages;
                    else
                        ApplyPagination();
                    
                    UpdateCounters();
                    ShowSuccessMessage("Pessoa excluída com sucesso!");
                }
                else
                {
                    ShowErrorMessage(result.Message ?? "Erro ao excluir pessoa.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao excluir: {ex.Message}");
            }
        }


        private void FilterByType(object? parameter)
        {
            if (parameter is PersonType type)
            {
                SelectedTypeFilter = type;
                _ = SearchAsync();
            }
        }

        private void ClearFilter()
        {
            SelectedTypeFilter = null;
            SearchText = string.Empty;
            _ = LoadDataAsync();
        }


        private void UpdateCounters()
        {
            OnPropertyChanged(nameof(TotalPersons));
            OnPropertyChanged(nameof(TotalCustomers));
            OnPropertyChanged(nameof(TotalSuppliers));
            OnPropertyChanged(nameof(TotalEmployees));
        }

        #endregion

        #region Paginação

        private void ApplyPagination()
        {
            var skip = (CurrentPage - 1) * PageSize;
            var pagedData = _allPersons.Skip(skip).Take(PageSize);
            Persons = new ObservableCollection<PersonDto>(pagedData);
            UpdatePaginationInfo();
        }

        private void UpdatePaginationInfo()
        {
            OnPropertyChanged(nameof(PageInfo));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private void GoToFirstPage()
        {
            CurrentPage = 1;
        }

        private void GoToPreviousPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }

        private void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }

        private void GoToLastPage()
        {
            CurrentPage = TotalPages;
        }

        private bool CanGoToPreviousPage()
        {
            return CurrentPage > 1;
        }

        private bool CanGoToNextPage()
        {
            return CurrentPage < TotalPages;
        }

        #endregion

        #region Mensagens

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
    }
}
