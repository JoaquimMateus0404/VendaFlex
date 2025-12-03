using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using System.Text;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Stock
{
    /// <summary>
    /// ViewModel para gerenciamento completo de estoque, incluindo
    /// movimentações, ajustes, alertas e relatórios.
    /// </summary>
    public class StockManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly IStockMovementService _stockMovementService;
        private readonly ICurrentUserContext _currentUserContext;

        #endregion

        #region Collections

        private ObservableCollection<StockDto> _stocks = new();
        public ObservableCollection<StockDto> Stocks
        {
            get => _stocks;
            set => Set(ref _stocks, value);
        }

        private ObservableCollection<StockDto> _filteredStocks = new();
        public ObservableCollection<StockDto> FilteredStocks
        {
            get => _filteredStocks;
            set => Set(ref _filteredStocks, value);
        }

        private ObservableCollection<StockDto> _lowStockProducts = new();
        public ObservableCollection<StockDto> LowStockProducts
        {
            get => _lowStockProducts;
            set => Set(ref _lowStockProducts, value);
        }

        private ObservableCollection<StockDto> _outOfStockProducts = new();
        public ObservableCollection<StockDto> OutOfStockProducts
        {
            get => _outOfStockProducts;
            set => Set(ref _outOfStockProducts, value);
        }

        private ObservableCollection<StockMovementDto> _movements = new();
        public ObservableCollection<StockMovementDto> Movements
        {
            get => _movements;
            set => Set(ref _movements, value);
        }

        private ObservableCollection<StockMovementDto> _filteredMovements = new();
        public ObservableCollection<StockMovementDto> FilteredMovements
        {
            get => _filteredMovements;
            set => Set(ref _filteredMovements, value);
        }

        private ObservableCollection<ProductDto> _products = new();
        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set => Set(ref _products, value);
        }

        #endregion

        #region Selected Items

        private StockDto? _selectedStock;
        public StockDto? SelectedStock
        {
            get => _selectedStock;
            set => Set(ref _selectedStock, value);
        }

        private StockMovementDto? _selectedMovement;
        public StockMovementDto? SelectedMovement
        {
            get => _selectedMovement;
            set => Set(ref _selectedMovement, value);
        }

        #endregion

        #region Statistics

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set => Set(ref _totalProducts, value);
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set => Set(ref _lowStockCount, value);
        }

        private int _outOfStockCount;
        public int OutOfStockCount
        {
            get => _outOfStockCount;
            set => Set(ref _outOfStockCount, value);
        }

        private decimal _totalStockValue;
        public decimal TotalStockValue
        {
            get => _totalStockValue;
            set => Set(ref _totalStockValue, value);
        }

        private int _totalMovementsToday;
        public int TotalMovementsToday
        {
            get => _totalMovementsToday;
            set => Set(ref _totalMovementsToday, value);
        }

        #endregion

        #region Filters

        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (Set(ref _searchTerm, value))
                    ApplyFilters();
            }
        }

        private bool _showLowStockOnly;
        public bool ShowLowStockOnly
        {
            get => _showLowStockOnly;
            set
            {
                if (Set(ref _showLowStockOnly, value))
                    ApplyFilters();
            }
        }

        private bool _showOutOfStockOnly;
        public bool ShowOutOfStockOnly
        {
            get => _showOutOfStockOnly;
            set
            {
                if (Set(ref _showOutOfStockOnly, value))
                    ApplyFilters();
            }
        }

        private string _movementSearchTerm = string.Empty;
        public string MovementSearchTerm
        {
            get => _movementSearchTerm;
            set
            {
                if (Set(ref _movementSearchTerm, value))
                    ApplyMovementFilters();
            }
        }

        private DateTime? _movementStartDate;
        public DateTime? MovementStartDate
        {
            get => _movementStartDate;
            set
            {
                if (Set(ref _movementStartDate, value))
                    ApplyMovementFilters();
            }
        }

        private DateTime? _movementEndDate;
        public DateTime? MovementEndDate
        {
            get => _movementEndDate;
            set
            {
                if (Set(ref _movementEndDate, value))
                    ApplyMovementFilters();
            }
        }

        private StockMovementType? _selectedMovementType;
        public StockMovementType? SelectedMovementType
        {
            get => _selectedMovementType;
            set
            {
                if (Set(ref _selectedMovementType, value))
                    ApplyMovementFilters();
            }
        }

        #endregion

        #region Pagination - Stocks

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (Set(ref _currentPage, value))
                    ApplyFilters();
            }
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (Set(ref _pageSize, value))
                {
                    CurrentPage = 1;
                    ApplyFilters();
                }
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => Set(ref _totalPages, value);
        }

        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set => Set(ref _totalRecords, value);
        }

        #endregion

        #region Pagination - Movements

        private int _movementCurrentPage = 1;
        public int MovementCurrentPage
        {
            get => _movementCurrentPage;
            set
            {
                if (Set(ref _movementCurrentPage, value))
                    ApplyMovementFilters();
            }
        }

        private int _movementPageSize = 20;
        public int MovementPageSize
        {
            get => _movementPageSize;
            set
            {
                if (Set(ref _movementPageSize, value))
                {
                    MovementCurrentPage = 1;
                    ApplyMovementFilters();
                }
            }
        }

        private int _movementTotalPages;
        public int MovementTotalPages
        {
            get => _movementTotalPages;
            set => Set(ref _movementTotalPages, value);
        }

        private int _movementTotalRecords;
        public int MovementTotalRecords
        {
            get => _movementTotalRecords;
            set => Set(ref _movementTotalRecords, value);
        }

        #endregion

        #region Movement Form

        private bool _isMovementFormOpen;
        public bool IsMovementFormOpen
        {
            get => _isMovementFormOpen;
            set => Set(ref _isMovementFormOpen, value);
        }

        private int _movementProductId;
        public int MovementProductId
        {
            get => _movementProductId;
            set => Set(ref _movementProductId, value);
        }

        private ProductDto? _selectedProduct;
        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (Set(ref _selectedProduct, value))
                {
                    if (value != null)
                        MovementProductId = value.ProductId;
                }
            }
        }

        private StockMovementType _movementType = StockMovementType.Entry;
        public StockMovementType MovementType
        {
            get => _movementType;
            set => Set(ref _movementType, value);
        }

        private int _movementQuantity;
        public int MovementQuantity
        {
            get => _movementQuantity;
            set => Set(ref _movementQuantity, value);
        }

        private string _movementReason = string.Empty;
        public string MovementReason
        {
            get => _movementReason;
            set => Set(ref _movementReason, value);
        }

        private string _movementNotes = string.Empty;
        public string MovementNotes
        {
            get => _movementNotes;
            set => Set(ref _movementNotes, value);
        }

        #endregion

        #region Adjustment Form

        private bool _isAdjustmentFormOpen;
        public bool IsAdjustmentFormOpen
        {
            get => _isAdjustmentFormOpen;
            set => Set(ref _isAdjustmentFormOpen, value);
        }

        private int _adjustmentProductId;
        public int AdjustmentProductId
        {
            get => _adjustmentProductId;
            set => Set(ref _adjustmentProductId, value);
        }

        private int _adjustmentNewQuantity;
        public int AdjustmentNewQuantity
        {
            get => _adjustmentNewQuantity;
            set => Set(ref _adjustmentNewQuantity, value);
        }

        private string _adjustmentReason = string.Empty;
        public string AdjustmentReason
        {
            get => _adjustmentReason;
            set => Set(ref _adjustmentReason, value);
        }

        #endregion

        #region Status Messages

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        private bool _isStatusMessageVisible;
        public bool IsStatusMessageVisible
        {
            get => _isStatusMessageVisible;
            set => Set(ref _isStatusMessageVisible, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private readonly DispatcherTimer _statusMessageTimer;

        #endregion

        #region Commands

        // Data Loading Commands
        public ICommand LoadStocksCommand { get; private set; } = null!;
        public ICommand LoadMovementsCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;

        // Movement Commands
        public ICommand AddMovementCommand { get; private set; } = null!;
        public ICommand SaveMovementCommand { get; private set; } = null!;
        public ICommand CancelMovementCommand { get; private set; } = null!;

        // Adjustment Commands
        public ICommand AdjustStockCommand { get; private set; } = null!;
        public ICommand SaveAdjustmentCommand { get; private set; } = null!;
        public ICommand CancelAdjustmentCommand { get; private set; } = null!;

        // Filter Commands
        public ICommand ClearFiltersCommand { get; private set; } = null!;
        public ICommand ClearMovementFiltersCommand { get; private set; } = null!;

        // Export Commands
        public ICommand ExportStocksCommand { get; private set; } = null!;
        public ICommand ExportMovementsCommand { get; private set; } = null!;

        // Pagination Commands - Stocks
        public ICommand FirstPageCommand { get; private set; } = null!;
        public ICommand PreviousPageCommand { get; private set; } = null!;
        public ICommand NextPageCommand { get; private set; } = null!;
        public ICommand LastPageCommand { get; private set; } = null!;

        // Pagination Commands - Movements
        public ICommand MovementFirstPageCommand { get; private set; } = null!;
        public ICommand MovementPreviousPageCommand { get; private set; } = null!;
        public ICommand MovementNextPageCommand { get; private set; } = null!;
        public ICommand MovementLastPageCommand { get; private set; } = null!;

        #endregion

        #region Constructor

        public StockManagementViewModel(
            IStockService stockService,
            IProductService productService,
            IStockMovementService stockMovementService,
            ICurrentUserContext currentUserContext)
        {
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));

            System.Diagnostics.Debug.WriteLine($"[VIEWMODEL CONSTRUCTOR] StockManagementViewModel criado");
            System.Diagnostics.Debug.WriteLine($"[VIEWMODEL CONSTRUCTOR] CurrentUserContext injetado: {_currentUserContext != null}");
            System.Diagnostics.Debug.WriteLine($"[VIEWMODEL CONSTRUCTOR] UserId no construtor: {_currentUserContext?.UserId}");

            _statusMessageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _statusMessageTimer.Tick += (s, e) =>
            {
                IsStatusMessageVisible = false;
                _statusMessageTimer.Stop();
            };

            InitializeCommands();
            _ = LoadDataAsync();
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            // Data Loading Commands
            LoadStocksCommand = new RelayCommand(async _ => await LoadStocksAsync());
            LoadMovementsCommand = new RelayCommand(async _ => await LoadMovementsAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());

            // Movement Commands
            // NOTA: Movimentações são criadas AUTOMATICAMENTE pelo sistema
            // Quando houver alterações no estoque, não devem ser criadas manualmente
            AddMovementCommand = new RelayCommand(_ => ShowStatusMessage("Movimentações são criadas automaticamente pelo sistema ao ajustar o estoque."));
            SaveMovementCommand = new RelayCommand(_ => { }, _ => false); // Desabilitado
            CancelMovementCommand = new RelayCommand(_ => CancelMovementForm());

            // Adjustment Commands
            AdjustStockCommand = new RelayCommand(_ => OpenAdjustmentForm(), _ => SelectedStock != null);
            SaveAdjustmentCommand = new RelayCommand(async _ => await SaveAdjustmentAsync(), _ => CanSaveAdjustment());
            CancelAdjustmentCommand = new RelayCommand(_ => CancelAdjustmentForm());

            // Filter Commands
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ClearMovementFiltersCommand = new RelayCommand(_ => ClearMovementFilters());

            // Export Commands
            ExportStocksCommand = new RelayCommand(async _ => await ExportStocksAsync(), _ => FilteredStocks.Any());
            ExportMovementsCommand = new RelayCommand(async _ => await ExportMovementsAsync(), _ => FilteredMovements.Any());

            // Pagination Commands - Stocks
            FirstPageCommand = new RelayCommand(_ => CurrentPage = 1, _ => CurrentPage > 1);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
            LastPageCommand = new RelayCommand(_ => CurrentPage = TotalPages, _ => CurrentPage < TotalPages);

            // Pagination Commands - Movements
            MovementFirstPageCommand = new RelayCommand(_ => MovementCurrentPage = 1, _ => MovementCurrentPage > 1);
            MovementPreviousPageCommand = new RelayCommand(_ => MovementCurrentPage--, _ => MovementCurrentPage > 1);
            MovementNextPageCommand = new RelayCommand(_ => MovementCurrentPage++, _ => MovementCurrentPage < MovementTotalPages);
            MovementLastPageCommand = new RelayCommand(_ => MovementCurrentPage = MovementTotalPages, _ => MovementCurrentPage < MovementTotalPages);
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                await LoadProductsAsync();
                await LoadStocksAsync();
                await LoadMovementsAsync();
                await LoadLowStockAsync();
                await LoadOutOfStockAsync();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar dados: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var result = await _productService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(result.Data);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar produtos: {ex.Message}");
            }
        }

        private async Task LoadStocksAsync()
        {
            try
            {
                var result = await _stockService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    Stocks = new ObservableCollection<StockDto>(result.Data);
                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar estoque: {ex.Message}");
            }
        }

        private async Task LoadMovementsAsync()
        {
            try
            {
                var result = await _stockMovementService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    Movements = new ObservableCollection<StockMovementDto>(
                        result.Data.OrderByDescending(m => m.Date));
                    ApplyMovementFilters();
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar movimentações: {ex.Message}");
            }
        }

        private async Task LoadLowStockAsync()
        {
            try
            {
                var result = await _stockService.GetLowStockAsync();
                if (result.Success && result.Data != null)
                {
                    LowStockProducts = new ObservableCollection<StockDto>(result.Data);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar estoque baixo: {ex.Message}");
            }
        }

        private async Task LoadOutOfStockAsync()
        {
            try
            {
                var result = await _stockService.GetOutOfStockAsync();
                if (result.Success && result.Data != null)
                {
                    OutOfStockProducts = new ObservableCollection<StockDto>(result.Data);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar produtos sem estoque: {ex.Message}");
            }
        }

        #endregion

        #region Filters

        private void ApplyFilters()
        {
            var filtered = Stocks.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(s =>
                    s.ProductName != null && s.ProductName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Low stock filter
            if (ShowLowStockOnly)
            {
                filtered = filtered.Where(s =>
                    s.MinimumStock.HasValue && s.Quantity <= s.MinimumStock.Value);
            }

            // Out of stock filter
            if (ShowOutOfStockOnly)
            {
                filtered = filtered.Where(s => s.Quantity == 0);
            }

            // Calculate pagination
            var filteredList = filtered.ToList();
            TotalRecords = filteredList.Count;
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);

            // Adjust current page if needed
            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;
            if (CurrentPage < 1)
                CurrentPage = 1;

            // Apply pagination
            var paginatedList = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            FilteredStocks = new ObservableCollection<StockDto>(paginatedList);
        }

        private void ApplyMovementFilters()
        {
            var filtered = Movements.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(MovementSearchTerm))
            {
                filtered = filtered.Where(m =>
                    (m.ProductName != null && m.ProductName.Contains(MovementSearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Notes != null && m.Notes.Contains(MovementSearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Date range filter
            if (MovementStartDate.HasValue)
            {
                filtered = filtered.Where(m => m.Date >= MovementStartDate.Value);
            }

            if (MovementEndDate.HasValue)
            {
                filtered = filtered.Where(m => m.Date <= MovementEndDate.Value.AddDays(1).AddSeconds(-1));
            }

            // Movement type filter
            if (SelectedMovementType.HasValue)
            {
                filtered = filtered.Where(m => m.Type == SelectedMovementType.Value);
            }

            // Calculate pagination
            var filteredList = filtered.ToList();
            MovementTotalRecords = filteredList.Count;
            MovementTotalPages = (int)Math.Ceiling((double)MovementTotalRecords / MovementPageSize);

            // Adjust current page if needed
            if (MovementCurrentPage > MovementTotalPages && MovementTotalPages > 0)
                MovementCurrentPage = MovementTotalPages;
            if (MovementCurrentPage < 1)
                MovementCurrentPage = 1;

            // Apply pagination
            var paginatedList = filteredList
                .Skip((MovementCurrentPage - 1) * MovementPageSize)
                .Take(MovementPageSize)
                .ToList();

            FilteredMovements = new ObservableCollection<StockMovementDto>(paginatedList);
        }

        private void ClearFilters()
        {
            SearchTerm = string.Empty;
            ShowLowStockOnly = false;
            ShowOutOfStockOnly = false;
            CurrentPage = 1;
        }

        private void ClearMovementFilters()
        {
            MovementSearchTerm = string.Empty;
            MovementStartDate = null;
            MovementEndDate = null;
            SelectedMovementType = null;
            MovementCurrentPage = 1;
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            TotalProducts = Stocks.Count;
            LowStockCount = LowStockProducts.Count;
            OutOfStockCount = OutOfStockProducts.Count;

            
            var productsDict = Products.ToDictionary(p => p.ProductId);
            TotalStockValue = Stocks
                .Where(s => productsDict.ContainsKey(s.ProductId))
                .Sum(s => productsDict[s.ProductId].SalePrice * s.Quantity);

            // Count today's movements
            var today = DateTime.Today;
            TotalMovementsToday = Movements.Count(m => m.Date.Date == today);
        }

        #endregion

        #region Movement Form

        private void OpenMovementForm()
        {
            ClearMovementForm();
            IsMovementFormOpen = true;
        }

        private bool CanSaveMovement()
        {
            return MovementProductId > 0 &&
                   MovementQuantity > 0 &&
                   !string.IsNullOrWhiteSpace(MovementReason);
        }

        /// <summary>
        /// MÉTODO DESABILITADO - Movimentações são criadas automaticamente pelo sistema.
        /// Use o método SaveAdjustmentAsync para ajustar o estoque, que criará a movimentação automaticamente.
        /// </summary>
        [Obsolete("Movimentações são criadas automaticamente. Use SaveAdjustmentAsync para ajustar estoque.")]
        private async Task SaveMovementAsync()
        {
            ShowStatusMessage("Movimentações são criadas automaticamente pelo sistema. Use 'Ajustar Estoque' para fazer alterações.");
            IsMovementFormOpen = false;
        }

        private void CancelMovementForm()
        {
            IsMovementFormOpen = false;
            ClearMovementForm();
        }

        private void ClearMovementForm()
        {
            MovementProductId = 0;
            SelectedProduct = null;
            MovementType = StockMovementType.Entry;
            MovementQuantity = 0;
            MovementReason = string.Empty;
            MovementNotes = string.Empty;
        }

        #endregion

        #region Adjustment Form

        private void OpenAdjustmentForm()
        {
            if (SelectedStock == null) return;

            AdjustmentProductId = SelectedStock.ProductId;
            AdjustmentNewQuantity = SelectedStock.Quantity;
            AdjustmentReason = string.Empty;
            IsAdjustmentFormOpen = true;
        }

        private bool CanSaveAdjustment()
        {
            return AdjustmentProductId > 0 &&
                   AdjustmentNewQuantity >= 0 &&
                   !string.IsNullOrWhiteSpace(AdjustmentReason);
        }

        private async Task SaveAdjustmentAsync()
        {
            try
            {
                IsLoading = true;
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveAdjustmentAsync - INICIADO");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ProductId: {AdjustmentProductId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Nova Quantidade: {AdjustmentNewQuantity}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Razão: {AdjustmentReason}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] _currentUserContext é null? {_currentUserContext == null}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UserId do contexto: {_currentUserContext?.UserId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UserId HasValue: {_currentUserContext?.UserId.HasValue}");
                
                if (_currentUserContext?.UserId == null || !_currentUserContext.UserId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ERRO CRÍTICO: UserId não está definido no contexto!");
                    ShowStatusMessage("Erro: Usuário não identificado. Faça login novamente.");
                    return;
                }

                var userId = _currentUserContext.UserId.Value;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UserId extraído: {userId}");

                // Buscar estoque atual
                var currentStock = await _stockService.GetByProductIdAsync(AdjustmentProductId);
                if (!currentStock.Success || currentStock.Data == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ERRO: Estoque não encontrado!");
                    ShowStatusMessage("Estoque não encontrado!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Estoque atual encontrado: Quantidade = {currentStock.Data.Quantity}");

                // Verificar se houve mudança
                if (currentStock.Data.Quantity == AdjustmentNewQuantity)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] AVISO: Quantidade não foi alterada!");
                    ShowStatusMessage("A quantidade não foi alterada!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Chamando UpdateQuantityAsync...");
                
                // Atualizar o estoque com a nota do usuário
                // O StockRepository criará automaticamente a movimentação correta:
                // - Entry (se aumentou a quantidade)
                // - Exit (se diminuiu a quantidade)
                var success = await _stockService.UpdateQuantityAsync(
                    AdjustmentProductId, 
                    AdjustmentNewQuantity, 
                    userId,
                    $"Ajuste de estoque: {AdjustmentReason}");

                System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateQuantityAsync retornou: {success}");

                if (success)
                {
                    ShowStatusMessage("✓ Ajuste de estoque realizado com sucesso!");
                    
                    // Fechar o modal
                    IsAdjustmentFormOpen = false;
                    
                    // Limpar o formulário
                    CancelAdjustmentForm();
                    
                    // Recarregar dados
                    await LoadDataAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveAdjustmentAsync - CONCLUÍDO COM SUCESSO");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ERRO: UpdateQuantityAsync retornou false");
                    ShowStatusMessage("✗ Erro ao ajustar estoque. Verifique os dados e tente novamente.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EXCEÇÃO em SaveAdjustmentAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] StackTrace: {ex.StackTrace}");
                ShowStatusMessage($"✗ Erro ao salvar ajuste: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelAdjustmentForm()
        {
            IsAdjustmentFormOpen = false;
            AdjustmentProductId = 0;
            AdjustmentNewQuantity = 0;
            AdjustmentReason = string.Empty;
        }

        #endregion

        #region Export

        private async Task ExportStocksAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Estoque_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();

                    if (extension == ".csv")
                        await ExportStocksToCsvAsync(dialog.FileName);
                    else if (extension == ".xlsx")
                        await ExportStocksToExcelAsync(dialog.FileName);

                    ShowStatusMessage("Exportação concluída com sucesso!");

                    // Open file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao exportar: {ex.Message}");
            }
        }

        private async Task ExportStocksToCsvAsync(string filePath)
        {
            await Task.Run(() =>
            {
                var csv = new StringBuilder();
                csv.AppendLine("Produto,Quantidade,Reservado,Disponível,Mínimo,Ponto de Reposição,Última Atualização");

                foreach (var stock in FilteredStocks)
                {
                    csv.AppendLine($"\"{stock.ProductName}\",{stock.Quantity},{stock.ReservedQuantity ?? 0}," +
                                   $"{stock.AvailableQuantity},{stock.MinimumStock ?? 0},{stock.ReorderPoint ?? 0}," +
                                   $"\"{stock.LastStockUpdate:dd/MM/yyyy HH:mm}\"");
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            });
        }

        private async Task ExportStocksToExcelAsync(string filePath)
        {
            // Placeholder for Excel export using EPPlus or ClosedXML
            await Task.Run(() =>
            {
                var tsv = new StringBuilder();
                tsv.AppendLine("Produto\tQuantidade\tReservado\tDisponível\tMínimo\tPonto de Reposição\tÚltima Atualização");

                foreach (var stock in FilteredStocks)
                {
                    tsv.AppendLine($"{stock.ProductName}\t{stock.Quantity}\t{stock.ReservedQuantity ?? 0}\t" +
                                   $"{stock.AvailableQuantity}\t{stock.MinimumStock ?? 0}\t{stock.ReorderPoint ?? 0}\t" +
                                   $"{stock.LastStockUpdate:dd/MM/yyyy HH:mm}");
                }

                File.WriteAllText(filePath, tsv.ToString(), Encoding.UTF8);
            });
        }

        private async Task ExportMovementsAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Movimentacoes_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();

                    if (extension == ".csv")
                        await ExportMovementsToCsvAsync(dialog.FileName);
                    else if (extension == ".xlsx")
                        await ExportMovementsToExcelAsync(dialog.FileName);

                    ShowStatusMessage("Exportação concluída com sucesso!");

                    // Open file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao exportar: {ex.Message}");
            }
        }

        private async Task ExportMovementsToCsvAsync(string filePath)
        {
            await Task.Run(() =>
            {
                var csv = new StringBuilder();
                csv.AppendLine("Data,Produto,Tipo,Quantidade,Motivo,Usuário,Observações");

                foreach (var movement in FilteredMovements)
                {
                    csv.AppendLine($"\"{movement.Date:dd/MM/yyyy HH:mm}\",\"{movement.ProductName}\"," +
                                   $"\"{movement.Type}\",{movement.Quantity},\"{movement.Notes}\"," +
                                   $"\"{movement.UserName}\",\"{movement.Notes}\"");
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            });
        }

        private async Task ExportMovementsToExcelAsync(string filePath)
        {
            await Task.Run(() =>
            {
                var tsv = new StringBuilder();
                tsv.AppendLine("Data\tProduto\tTipo\tQuantidade\tMotivo\tUsuário\tObservações");

                foreach (var movement in FilteredMovements)
                {
                    tsv.AppendLine($"{movement.Date:dd/MM/yyyy HH:mm}\t{movement.ProductName}\t" +
                                   $"{movement.Type}\t{movement.Quantity}\t{movement.Notes}\t" +
                                   $"{movement.UserName}\t{movement.Reference}");
                }

                File.WriteAllText(filePath, tsv.ToString(), Encoding.UTF8);
            });
        }

        #endregion

        #region Status Messages

        private void ShowStatusMessage(string message)
        {
            StatusMessage = message;
            IsStatusMessageVisible = true;
            _statusMessageTimer.Stop();
            _statusMessageTimer.Start();
        }

        #endregion
    }
}
