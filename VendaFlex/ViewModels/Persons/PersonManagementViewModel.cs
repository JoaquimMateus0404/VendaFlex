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
        private readonly IFileStorageService _fileStorageService;
        private ObservableCollection<PersonDto> _persons;
        private ObservableCollection<PersonDto> _allPersons;
        private PersonDto? _selectedPerson;
        private string _searchText = string.Empty;
        private PersonType? _selectedTypeFilter;
        private bool _isLoading;
        private bool _isEditMode;
        private bool _showForm;
        private string _message = string.Empty;
        private bool _isMessageError;
        private bool _showMessage;
        
        // Paginação
        private int _currentPage = 1;
        private int _pageSize = 20;
        private int _totalPages = 1;
        private int _totalItems = 0;

        // Propriedades do formulário
        private int _personId;
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

        public PersonManagementViewModel(IPersonService personService, IFileStorageService fileStorageService)
        {
            _personService = personService;
            _fileStorageService = fileStorageService;
            _persons = new ObservableCollection<PersonDto>();
            _allPersons = new ObservableCollection<PersonDto>();

            // Comandos
            LoadDataCommand = new AsyncCommand(LoadDataAsync);
            SearchCommand = new AsyncCommand(SearchAsync);
            AddCommand = new RelayCommand(_ => ShowAddForm());
            EditCommand = new RelayCommand(_ => ShowEditForm(), _ => SelectedPerson != null);
            DeleteCommand = new AsyncCommand(DeletePersonAsync, () => SelectedPerson != null);
            SaveCommand = new AsyncCommand(SavePersonAsync, CanSave);
            CancelCommand = new RelayCommand(_ => CancelForm());
            FilterByTypeCommand = new RelayCommand(param => FilterByType(param));
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            UploadPhotoCommand = new AsyncCommand(UploadPhotoAsync);
            
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

        // Propriedades do formulário
        public int PersonId
        {
            get => _personId;
            set => Set(ref _personId, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                if (Set(ref _name, value))
                    ((AsyncCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public PersonType Type
        {
            get => _type;
            set => Set(ref _type, value);
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
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FilterByTypeCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand UploadPhotoCommand { get; }
        
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
            ClearForm();
            IsEditMode = false;
            ShowForm = true;
        }

        private void ShowEditForm()
        {
            if (SelectedPerson == null) return;

            LoadPersonToForm(SelectedPerson);
            IsEditMode = true;
            ShowForm = true;
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

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private async Task SavePersonAsync()
        {
            try
            {
                HideMessage();

                if (string.IsNullOrWhiteSpace(Name))
                {
                    ShowErrorMessage("O nome é obrigatório.");
                    return;
                }

                var personDto = new PersonDto
                {
                    PersonId = PersonId,
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

                if (IsEditMode)
                {
                    var result = await _personService.UpdateAsync(personDto);
                    if (result.Success && result.Data != null)
                    {
                        var index = _allPersons.ToList().FindIndex(p => p.PersonId == result.Data.PersonId);
                        if (index >= 0)
                        {
                            _allPersons[index] = result.Data;
                        }
                        ApplyPagination();
                        ShowSuccessMessage("Pessoa atualizada com sucesso!");
                    }
                    else
                    {
                        ShowErrorMessage(result.Message ?? "Erro ao atualizar pessoa.");
                        return;
                    }
                }
                else
                {
                    var result = await _personService.CreateAsync(personDto);
                    if (result.Success && result.Data != null)
                    {
                        _allPersons.Add(result.Data);
                        TotalItems = _allPersons.Count;
                        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                        ApplyPagination();
                        ShowSuccessMessage("Pessoa cadastrada com sucesso!");
                    }
                    else
                    {
                        ShowErrorMessage(result.Message ?? "Erro ao criar pessoa.");
                        return;
                    }
                }

                UpdateCounters();
                ShowForm = false;
                ClearForm();
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

        private void LoadPersonToForm(PersonDto person)
        {
            PersonId = person.PersonId;
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

        private void ClearForm()
        {
            PersonId = 0;
            Name = string.Empty;
            Type = PersonType.Customer;
            TaxId = string.Empty;
            IdentificationNumber = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            MobileNumber = string.Empty;
            Website = string.Empty;
            Address = string.Empty;
            City = string.Empty;
            State = string.Empty;
            PostalCode = string.Empty;
            Country = string.Empty;
            CreditLimit = 0;
            CurrentBalance = 0;
            ContactPerson = string.Empty;
            Notes = string.Empty;
            ProfileImageUrl = string.Empty;
            IsActive = true;
            Rating = null;
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

        #region Upload de Foto

        private async Task UploadPhotoAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Selecionar Foto da Pessoa",
                    Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos os ficheiros|*.*",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    HideMessage();

                    // Validar se é uma imagem válida
                    if (!_fileStorageService.IsValidImage(dialog.FileName))
                    {
                        ShowErrorMessage("O ficheiro selecionado não é uma imagem válida.");
                        IsLoading = false;
                        return;
                    }

                    // Salvar a foto
                    var savedPath = await _fileStorageService.SaveLogoAsync(dialog.FileName);
                    
                    // Remover foto antiga se existir
                    if (!string.IsNullOrEmpty(ProfileImageUrl) && System.IO.File.Exists(ProfileImageUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(ProfileImageUrl);
                    }

                    // Atualizar a URL da foto
                    ProfileImageUrl = savedPath;
                    ShowSuccessMessage("Foto carregada com sucesso!");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao carregar foto: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}
