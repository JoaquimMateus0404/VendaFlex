using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Products
{
    /// <summary>
    /// ViewModel para gerenciamento completo de produtos, categorias, estoque,
    /// validades, movimentações e histórico de preços.
    /// </summary>
    public class ProductManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStockService _stockService;
        private readonly IExpirationService _expirationService;
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IPersonService _personService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly DispatcherTimer _statusMessageTimer;

        #endregion

        #region Constructor

        public ProductManagementViewModel(
            IProductService productService,
            ICategoryService categoryService,
            IStockService stockService,
            IExpirationService expirationService,
            IPriceHistoryService priceHistoryService,
            IStockMovementService stockMovementService,
            IPersonService personService,
            ICurrentUserContext currentUserContext,
            IFileStorageService fileStorageService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _expirationService = expirationService ?? throw new ArgumentNullException(nameof(expirationService));
            _priceHistoryService = priceHistoryService ?? throw new ArgumentNullException(nameof(priceHistoryService));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));

            // Initialize timer for status messages
            _statusMessageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
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

        private ObservableCollection<ProductDto> _products = new();
        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set => Set(ref _products, value);
        }

        private ObservableCollection<ProductDto> _filteredProducts = new();
        public ObservableCollection<ProductDto> FilteredProducts
        {
            get => _filteredProducts;
            set => Set(ref _filteredProducts, value);
        }

        private ObservableCollection<CategoryDto> _categories = new();
        public ObservableCollection<CategoryDto> Categories
        {
            get => _categories;
            set => Set(ref _categories, value);
        }

        private ObservableCollection<PersonDto> _suppliers = new();
        public ObservableCollection<PersonDto> Suppliers
        {
            get => _suppliers;
            set => Set(ref _suppliers, value);
        }

        private ObservableCollection<StockDto> _lowStockProducts = new();
        public ObservableCollection<StockDto> LowStockProducts
        {
            get => _lowStockProducts;
            set => Set(ref _lowStockProducts, value);
        }

        private ObservableCollection<ExpirationDto> _expirations = new();
        public ObservableCollection<ExpirationDto> Expirations
        {
            get => _expirations;
            set => Set(ref _expirations, value);
        }

        private ObservableCollection<ExpirationDto> _nearExpirations = new();
        public ObservableCollection<ExpirationDto> NearExpirations
        {
            get => _nearExpirations;
            set => Set(ref _nearExpirations, value);
        }

        private ObservableCollection<StockMovementDto> _stockMovements = new();
        public ObservableCollection<StockMovementDto> StockMovements
        {
            get => _stockMovements;
            set => Set(ref _stockMovements, value);
        }

        private ObservableCollection<PriceHistoryDto> _priceHistories = new();
        public ObservableCollection<PriceHistoryDto> PriceHistories
        {
            get => _priceHistories;
            set => Set(ref _priceHistories, value);
        }

        #endregion

        #region Selected Items

        private ProductDto? _selectedProduct;
        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (Set(ref _selectedProduct, value) && value != null)
                {
                    _ = LoadProductDetailsAsync(value.ProductId);
                }
            }
        }

        private CategoryDto? _selectedCategory;
        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (Set(ref _selectedCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        private ExpirationDto? _selectedExpiration;
        public ExpirationDto? SelectedExpiration
        {
            get => _selectedExpiration;
            set => Set(ref _selectedExpiration, value);
        }

        private StockMovementDto? _selectedStockMovement;
        public StockMovementDto? SelectedStockMovement
        {
            get => _selectedStockMovement;
            set => Set(ref _selectedStockMovement, value);
        }

        #endregion

        #region Product Properties (for Add/Edit)

        private int _productId;
        public int ProductId
        {
            get => _productId;
            set => Set(ref _productId, value);
        }

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set
            {
                if (Set(ref _productName, value))
                    ValidateProductName();
            }
        }

        private string? _productDescription;
        public string? ProductDescription
        {
            get => _productDescription;
            set => Set(ref _productDescription, value);
        }

        private string? _productBarcode;
        public string? ProductBarcode
        {
            get => _productBarcode;
            set => Set(ref _productBarcode, value);
        }

        private string? _productSKU;
        public string? ProductSKU
        {
            get => _productSKU;
            set => Set(ref _productSKU, value);
        }

        private int _productCategoryId;
        public int ProductCategoryId
        {
            get => _productCategoryId;
            set => Set(ref _productCategoryId, value);
        }

        private decimal _productSalePrice;
        public decimal ProductSalePrice
        {
            get => _productSalePrice;
            set
            {
                if (Set(ref _productSalePrice, value))
                {
                    CalculateProfitMargin();
                    ValidatePrice();
                }
            }
        }

        private decimal _productCostPrice;
        public decimal ProductCostPrice
        {
            get => _productCostPrice;
            set
            {
                if (Set(ref _productCostPrice, value))
                {
                    CalculateProfitMargin();
                    ValidatePrice();
                }
            }
        }

        private decimal? _productDiscountPercentage;
        public decimal? ProductDiscountPercentage
        {
            get => _productDiscountPercentage;
            set
            {
                if (Set(ref _productDiscountPercentage, value))
                    CalculateFinalPrice();
            }
        }

        private decimal _profitMargin;
        public decimal ProfitMargin
        {
            get => _profitMargin;
            set => Set(ref _profitMargin, value);
        }

        private decimal _finalPrice;
        public decimal FinalPrice
        {
            get => _finalPrice;
            set => Set(ref _finalPrice, value);
        }

        private string? _productPhotoUrl;
        public string? ProductPhotoUrl
        {
            get => _productPhotoUrl;
            set => Set(ref _productPhotoUrl, value);
        }

        private ProductStatus _productStatus = ProductStatus.Active;
        public ProductStatus ProductStatus
        {
            get => _productStatus;
            set => Set(ref _productStatus, value);
        }

        private bool _productIsFeatured;
        public bool ProductIsFeatured
        {
            get => _productIsFeatured;
            set => Set(ref _productIsFeatured, value);
        }

        private bool _productControlsStock = true;
        public bool ProductControlsStock
        {
            get => _productControlsStock;
            set => Set(ref _productControlsStock, value);
        }

        private int? _productMinimumStock;
        public int? ProductMinimumStock
        {
            get => _productMinimumStock;
            set => Set(ref _productMinimumStock, value);
        }

        private int? _productReorderPoint;
        public int? ProductReorderPoint
        {
            get => _productReorderPoint;
            set => Set(ref _productReorderPoint, value);
        }

        private bool _productHasExpirationDate;
        public bool ProductHasExpirationDate
        {
            get => _productHasExpirationDate;
            set => Set(ref _productHasExpirationDate, value);
        }

        private string? _productCode;
        public string? ProductCode
        {
            get => _productCode;
            set => Set(ref _productCode, value);
        }

        private string? _productShortDescription;
        public string? ProductShortDescription
        {
            get => _productShortDescription;
            set => Set(ref _productShortDescription, value);
        }

        private string? _productWeight;
        public string? ProductWeight
        {
            get => _productWeight;
            set => Set(ref _productWeight, value);
        }

        private string? _productDimensions;
        public string? ProductDimensions
        {
            get => _productDimensions;
            set => Set(ref _productDimensions, value);
        }

        private int _productSupplierId;
        public int ProductSupplierId
        {
            get => _productSupplierId;
            set => Set(ref _productSupplierId, value);
        }

        private decimal? _productTaxRate;
        public decimal? ProductTaxRate
        {
            get => _productTaxRate;
            set => Set(ref _productTaxRate, value);
        }

        private bool _productAllowBackorder;
        public bool ProductAllowBackorder
        {
            get => _productAllowBackorder;
            set => Set(ref _productAllowBackorder, value);
        }

        private int _productDisplayOrder;
        public int ProductDisplayOrder
        {
            get => _productDisplayOrder;
            set => Set(ref _productDisplayOrder, value);
        }

        private int? _productMaximumStock;
        public int? ProductMaximumStock
        {
            get => _productMaximumStock;
            set => Set(ref _productMaximumStock, value);
        }

        private int? _productExpirationDays;
        public int? ProductExpirationDays
        {
            get => _productExpirationDays;
            set => Set(ref _productExpirationDays, value);
        }

        private int? _productExpirationWarningDays;
        public int? ProductExpirationWarningDays
        {
            get => _productExpirationWarningDays;
            set => Set(ref _productExpirationWarningDays, value);
        }

        #endregion

        #region Category Properties (for Add/Edit)

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set => Set(ref _categoryId, value);
        }

        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (Set(ref _categoryName, value))
                    ValidateCategoryName();
            }
        }

        private string? _categoryDescription;
        public string? CategoryDescription
        {
            get => _categoryDescription;
            set => Set(ref _categoryDescription, value);
        }

        private string? _categoryCode;
        public string? CategoryCode
        {
            get => _categoryCode;
            set => Set(ref _categoryCode, value);
        }

        private bool _categoryIsActive = true;
        public bool CategoryIsActive
        {
            get => _categoryIsActive;
            set => Set(ref _categoryIsActive, value);
        }

        #endregion

        #region Expiration Properties (for Add/Edit)

        private int _expirationId;
        public int ExpirationId
        {
            get => _expirationId;
            set => Set(ref _expirationId, value);
        }

        private int _expirationProductId;
        public int ExpirationProductId
        {
            get => _expirationProductId;
            set => Set(ref _expirationProductId, value);
        }

        private DateTime _expirationDate = DateTime.Now.AddMonths(6);
        public DateTime ExpirationDate
        {
            get => _expirationDate;
            set => Set(ref _expirationDate, value);
        }

        private int _expirationQuantity;
        public int ExpirationQuantity
        {
            get => _expirationQuantity;
            set => Set(ref _expirationQuantity, value);
        }

        private string? _batchNumber;
        public string? BatchNumber
        {
            get => _batchNumber;
            set => Set(ref _batchNumber, value);
        }

        private string? _expirationNotes;
        public string? ExpirationNotes
        {
            get => _expirationNotes;
            set => Set(ref _expirationNotes, value);
        }

        #endregion

        #region Stock Movement Properties

        private int _movementProductId;
        public int MovementProductId
        {
            get => _movementProductId;
            set => Set(ref _movementProductId, value);
        }

        private int _movementQuantity;
        public int MovementQuantity
        {
            get => _movementQuantity;
            set => Set(ref _movementQuantity, value);
        }

        private StockMovementType _movementType = StockMovementType.Entry;
        public StockMovementType MovementType
        {
            get => _movementType;
            set => Set(ref _movementType, value);
        }

        private string? _movementNotes;
        public string? MovementNotes
        {
            get => _movementNotes;
            set => Set(ref _movementNotes, value);
        }

        private decimal? _movementUnitCost;
        public decimal? MovementUnitCost
        {
            get => _movementUnitCost;
            set
            {
                if (Set(ref _movementUnitCost, value))
                    CalculateMovementTotalCost();
            }
        }

        private decimal? _movementTotalCost;
        public decimal? MovementTotalCost
        {
            get => _movementTotalCost;
            set => Set(ref _movementTotalCost, value);
        }

        #endregion

        #region Filters and Search

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Set(ref _searchText, value))
                    ApplyFilters();
            }
        }

        private ProductStatus? _filterStatus;
        public ProductStatus? FilterStatus
        {
            get => _filterStatus;
            set
            {
                if (Set(ref _filterStatus, value))
                    ApplyFilters();
            }
        }

        private bool _filterLowStock;
        public bool FilterLowStock
        {
            get => _filterLowStock;
            set
            {
                if (Set(ref _filterLowStock, value))
                    ApplyFilters();
            }
        }

        private bool _filterFeatured;
        public bool FilterFeatured
        {
            get => _filterFeatured;
            set
            {
                if (Set(ref _filterFeatured, value))
                    ApplyFilters();
            }
        }

        #endregion

        #region UI State

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

        private bool _isProductFormOpen;
        public bool IsProductFormOpen
        {
            get => _isProductFormOpen;
            set => Set(ref _isProductFormOpen, value);
        }

        private bool _isCategoryFormOpen;
        public bool IsCategoryFormOpen
        {
            get => _isCategoryFormOpen;
            set => Set(ref _isCategoryFormOpen, value);
        }

        private bool _isExpirationFormOpen;
        public bool IsExpirationFormOpen
        {
            get => _isExpirationFormOpen;
            set => Set(ref _isExpirationFormOpen, value);
        }

        private bool _isMovementFormOpen;
        public bool IsMovementFormOpen
        {
            get => _isMovementFormOpen;
            set => Set(ref _isMovementFormOpen, value);
        }

        private bool _isProductNameValid = true;
        public bool IsProductNameValid
        {
            get => _isProductNameValid;
            set => Set(ref _isProductNameValid, value);
        }

        private bool _isPriceValid = true;
        public bool IsPriceValid
        {
            get => _isPriceValid;
            set => Set(ref _isPriceValid, value);
        }

        private bool _isCategoryNameValid = true;
        public bool IsCategoryNameValid
        {
            get => _isCategoryNameValid;
            set => Set(ref _isCategoryNameValid, value);
        }

        #endregion

        #region Pagination

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

        #region Statistics

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set => Set(ref _totalProducts, value);
        }

        private int _activeProducts;
        public int ActiveProducts
        {
            get => _activeProducts;
            set => Set(ref _activeProducts, value);
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set => Set(ref _lowStockCount, value);
        }

        private int _expiredCount;
        public int ExpiredCount
        {
            get => _expiredCount;
            set => Set(ref _expiredCount, value);
        }

        private int _nearExpirationCount;
        public int NearExpirationCount
        {
            get => _nearExpirationCount;
            set => Set(ref _nearExpirationCount, value);
        }

        #endregion

        #region Commands

        // Product Commands
        public ICommand LoadProductsCommand { get; private set; } = null!;
        public ICommand AddProductCommand { get; private set; } = null!;
        public ICommand EditProductCommand { get; private set; } = null!;
        public ICommand SaveProductCommand { get; private set; } = null!;
        public ICommand DeleteProductCommand { get; private set; } = null!;
        public ICommand CancelProductCommand { get; private set; } = null!;
        public ICommand UploadProductPhotoCommand { get; private set; } = null!;
        public ICommand RemoveProductPhotoCommand { get; private set; } = null!;

        // Category Commands
        public ICommand LoadCategoriesCommand { get; private set; } = null!;
        public ICommand AddCategoryCommand { get; private set; } = null!;
        public ICommand EditCategoryCommand { get; private set; } = null!;
        public ICommand SaveCategoryCommand { get; private set; } = null!;
        public ICommand DeleteCategoryCommand { get; private set; } = null!;
        public ICommand CancelCategoryCommand { get; private set; } = null!;

        // Expiration Commands
        public ICommand LoadExpirationsCommand { get; private set; } = null!;
        public ICommand AddExpirationCommand { get; private set; } = null!;
        public ICommand EditExpirationCommand { get; private set; } = null!;
        public ICommand SaveExpirationCommand { get; private set; } = null!;
        public ICommand DeleteExpirationCommand { get; private set; } = null!;
        public ICommand CancelExpirationCommand { get; private set; } = null!;

        // Stock Movement Commands
        public ICommand LoadMovementsCommand { get; private set; } = null!;
        public ICommand AddMovementCommand { get; private set; } = null!;
        public ICommand SaveMovementCommand { get; private set; } = null!;
        public ICommand CancelMovementCommand { get; private set; } = null!;

        // Filter Commands
        public ICommand ClearFiltersCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;
        
        // Export Commands
        public ICommand ExportProductsCommand { get; private set; } = null!;
        
        // Pagination Commands
        public ICommand FirstPageCommand { get; private set; } = null!;
        public ICommand PreviousPageCommand { get; private set; } = null!;
        public ICommand NextPageCommand { get; private set; } = null!;
        public ICommand LastPageCommand { get; private set; } = null!;

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            // Product Commands
            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            AddProductCommand = new RelayCommand(_ => OpenProductForm());
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
            SaveProductCommand = new RelayCommand(async _ => await SaveProductAsync(), _ => CanSaveProduct());
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);
            CancelProductCommand = new RelayCommand(_ => CancelProductForm());
            UploadProductPhotoCommand = new RelayCommand(_ => UploadProductPhoto());
            RemoveProductPhotoCommand = new RelayCommand(_ => RemoveProductPhoto(), _ => !string.IsNullOrEmpty(ProductPhotoUrl));

            // Category Commands
            LoadCategoriesCommand = new RelayCommand(async _ => await LoadCategoriesAsync());
            AddCategoryCommand = new RelayCommand(_ => OpenCategoryForm());
            EditCategoryCommand = new RelayCommand(_ => EditCategory(), _ => SelectedCategory != null);
            SaveCategoryCommand = new RelayCommand(async _ => await SaveCategoryAsync(), _ => CanSaveCategory());
            DeleteCategoryCommand = new RelayCommand(async _ => await DeleteCategoryAsync(), _ => SelectedCategory != null);
            CancelCategoryCommand = new RelayCommand(_ => CancelCategoryForm());

            // Expiration Commands
            LoadExpirationsCommand = new RelayCommand(async _ => await LoadExpirationsAsync());
            AddExpirationCommand = new RelayCommand(_ => OpenExpirationForm(), _ => SelectedProduct != null);
            EditExpirationCommand = new RelayCommand(_ => EditExpiration(), _ => SelectedExpiration != null);
            SaveExpirationCommand = new RelayCommand(async _ => await SaveExpirationAsync(), _ => CanSaveExpiration());
            DeleteExpirationCommand = new RelayCommand(async _ => await DeleteExpirationAsync(), _ => SelectedExpiration != null);
            CancelExpirationCommand = new RelayCommand(_ => CancelExpirationForm());

            // Stock Movement Commands
            LoadMovementsCommand = new RelayCommand(async _ => await LoadStockMovementsAsync());
            AddMovementCommand = new RelayCommand(_ => OpenMovementForm(), _ => SelectedProduct != null);
            SaveMovementCommand = new RelayCommand(async _ => await SaveMovementAsync(), _ => CanSaveMovement());
            CancelMovementCommand = new RelayCommand(_ => CancelMovementForm());

            // Filter Commands
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            
            // Export Commands
            ExportProductsCommand = new RelayCommand(async _ => await ExportProductsAsync(), _ => FilteredProducts.Any());
            
            // Pagination Commands
            FirstPageCommand = new RelayCommand(_ => CurrentPage = 1, _ => CurrentPage > 1);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
            LastPageCommand = new RelayCommand(_ => CurrentPage = TotalPages, _ => CurrentPage < TotalPages);
        }

        private void InitializeCollections()
        {
            Products = new ObservableCollection<ProductDto>();
            FilteredProducts = new ObservableCollection<ProductDto>();
            Categories = new ObservableCollection<CategoryDto>();
            LowStockProducts = new ObservableCollection<StockDto>();
            Expirations = new ObservableCollection<ExpirationDto>();
            NearExpirations = new ObservableCollection<ExpirationDto>();
            StockMovements = new ObservableCollection<StockMovementDto>();
            PriceHistories = new ObservableCollection<PriceHistoryDto>();
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                // Executar sequencialmente para evitar problemas de concorrência no DbContext
                await LoadProductsAsync();
                await LoadCategoriesAsync();
                await LoadSuppliersAsync();
                await LoadLowStockAsync();
                await LoadExpirationsAsync();
                await LoadStockMovementsAsync();
                await LoadPriceHistoriesAsync();

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao carregar dados: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProductsAsync()
        {
            var result = await _productService.GetAllAsync();

            if (result.Success && result.Data != null)
            {
                Products.Clear();
                foreach (var product in result.Data)
                {
                    Products.Add(product);
                }

                ApplyFilters();
            }
            else
            {
                var errorMsg = result.Errors?.FirstOrDefault() ?? result.Message ?? "Erro ao carregar produtos";
                ShowStatusMessage(errorMsg, true);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var result = await _categoryService.GetAllAsync();

            if (result.Success && result.Data != null)
            {
                Categories.Clear();
                foreach (var category in result.Data)
                {
                    Categories.Add(category);
                }
            }
        }

        private async Task LoadSuppliersAsync()
        {
            // Carregar apenas pessoas do tipo Supplier (fornecedores)
            var result = await _personService.GetByTypeAsync(PersonType.Supplier);

            if (result.Success && result.Data != null)
            {
                Suppliers.Clear();
                foreach (var supplier in result.Data)
                {
                    Suppliers.Add(supplier);
                }
            }
        }

        private async Task LoadLowStockAsync()
        {
            var result = await _stockService.GetLowStockAsync();

            if (result.Success && result.Data != null)
            {
                LowStockProducts.Clear();
                foreach (var stock in result.Data)
                {
                    LowStockProducts.Add(stock);
                }
            }
        }

        private async Task LoadExpirationsAsync()
        {
            var allResult = await _expirationService.GetAllAsync();
            var nearResult = await _expirationService.GetNearExpirationAsync();

            if (allResult.Success && allResult.Data != null)
            {
                Expirations.Clear();
                foreach (var expiration in allResult.Data.OrderBy(e => e.ExpirationDate))
                {
                    Expirations.Add(expiration);
                }
            }

            if (nearResult.Success && nearResult.Data != null)
            {
                NearExpirations.Clear();
                foreach (var expiration in nearResult.Data)
                {
                    NearExpirations.Add(expiration);
                }
            }
        }

        private async Task LoadStockMovementsAsync()
        {
            var result = await _stockMovementService.GetAllAsync();

            if (result.Success && result.Data != null)
            {
                StockMovements.Clear();
                foreach (var movement in result.Data.OrderByDescending(m => m.Date).Take(100))
                {
                    StockMovements.Add(movement);
                }
            }
        }

        private async Task LoadPriceHistoriesAsync()
        {
            var result = await _priceHistoryService.GetAllAsync();

            if (result.Success && result.Data != null)
            {
                PriceHistories.Clear();
                foreach (var history in result.Data.OrderByDescending(h => h.ChangeDate).Take(100))
                {
                    PriceHistories.Add(history);
                }
            }
        }

        private async Task LoadProductDetailsAsync(int productId)
        {
            // Load stock movements for selected product
            var movementsResult = await _stockMovementService.GetByProductIdAsync(productId);
            if (movementsResult.Success && movementsResult.Data != null)
            {
                StockMovements.Clear();
                foreach (var movement in movementsResult.Data.OrderByDescending(m => m.Date))
                {
                    StockMovements.Add(movement);
                }
            }

            // Load price history for selected product
            var priceHistoryResult = await _priceHistoryService.GetByProductIdAsync(productId);
            if (priceHistoryResult.Success && priceHistoryResult.Data != null)
            {
                PriceHistories.Clear();
                foreach (var history in priceHistoryResult.Data.OrderByDescending(h => h.ChangeDate))
                {
                    PriceHistories.Add(history);
                }
            }

            // Load expirations for selected product
            var expirationsResult = await _expirationService.GetByProductIdAsync(productId);
            if (expirationsResult.Success && expirationsResult.Data != null)
            {
                Expirations.Clear();
                foreach (var expiration in expirationsResult.Data.OrderBy(e => e.ExpirationDate))
                {
                    Expirations.Add(expiration);
                }
            }
        }

        #endregion

        #region Product Methods

        private void OpenProductForm()
        {
            ClearProductForm();
            IsProductFormOpen = true;
        }

        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            ProductId = SelectedProduct.ProductId;
            ProductCode = SelectedProduct.Code;
            ProductName = SelectedProduct.Name;
            ProductDescription = SelectedProduct.Description;
            ProductShortDescription = SelectedProduct.ShortDescription;
            ProductBarcode = SelectedProduct.Barcode;
            ProductSKU = SelectedProduct.SKU;
            ProductWeight = SelectedProduct.Weight;
            ProductDimensions = SelectedProduct.Dimensions;
            ProductCategoryId = SelectedProduct.CategoryId;
            ProductSupplierId = SelectedProduct.SupplierId;
            ProductSalePrice = SelectedProduct.SalePrice;
            ProductCostPrice = SelectedProduct.CostPrice;
            ProductDiscountPercentage = SelectedProduct.DiscountPercentage;
            ProductTaxRate = SelectedProduct.TaxRate;
            ProductPhotoUrl = SelectedProduct.PhotoUrl;
            ProductStatus = SelectedProduct.Status;
            ProductIsFeatured = SelectedProduct.IsFeatured;
            ProductAllowBackorder = SelectedProduct.AllowBackorder;
            ProductDisplayOrder = SelectedProduct.DisplayOrder;
            ProductControlsStock = SelectedProduct.ControlsStock;
            ProductMinimumStock = SelectedProduct.MinimumStock;
            ProductMaximumStock = SelectedProduct.MaximumStock;
            ProductReorderPoint = SelectedProduct.ReorderPoint;
            ProductHasExpirationDate = SelectedProduct.HasExpirationDate;
            ProductExpirationDays = SelectedProduct.ExpirationDays;
            ProductExpirationWarningDays = SelectedProduct.ExpirationWarningDays;

            CalculateProfitMargin();
            CalculateFinalPrice();

            IsProductFormOpen = true;
        }

        private async Task SaveProductAsync()
        {
            if (!CanSaveProduct()) return;

            IsSaving = true;

            try
            {
                var productDto = new ProductDto
                {
                    ProductId = ProductId,
                    Code = ProductCode ?? string.Empty,
                    Name = ProductName,
                    Description = ProductDescription ?? string.Empty,
                    ShortDescription = ProductShortDescription ?? string.Empty,
                    Barcode = ProductBarcode ?? string.Empty,
                    SKU = ProductSKU ?? string.Empty,
                    Weight = ProductWeight ?? string.Empty,
                    Dimensions = ProductDimensions ?? string.Empty,
                    CategoryId = ProductCategoryId,
                    SupplierId = ProductSupplierId,
                    SalePrice = ProductSalePrice,
                    CostPrice = ProductCostPrice,
                    DiscountPercentage = ProductDiscountPercentage,
                    TaxRate = ProductTaxRate,
                    PhotoUrl = ProductPhotoUrl ?? string.Empty,
                    Status = ProductStatus,
                    IsFeatured = ProductIsFeatured,
                    AllowBackorder = ProductAllowBackorder,
                    DisplayOrder = ProductDisplayOrder,
                    ControlsStock = ProductControlsStock,
                    MinimumStock = ProductMinimumStock,
                    MaximumStock = ProductMaximumStock,
                    ReorderPoint = ProductReorderPoint,
                    HasExpirationDate = ProductHasExpirationDate,
                    ExpirationDays = ProductExpirationDays,
                    ExpirationWarningDays = ProductExpirationWarningDays
                };

                var result = ProductId == 0
                    ? await _productService.AddAsync(productDto)
                    : await _productService.UpdateAsync(productDto);

                if (result.Success)
                {
                    ShowStatusMessage(ProductId == 0 ? "Produto criado com sucesso!" : "Produto atualizado com sucesso!", false);
                    IsProductFormOpen = false;
                    await LoadProductsAsync();
                }
                else
                {
                    var errorMsg = result.Errors?.FirstOrDefault() ?? result.Message ?? "Erro ao salvar produto";
                    ShowStatusMessage(errorMsg, true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao salvar produto: {ex.Message}", true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show(
                $"Deseja realmente excluir o produto '{SelectedProduct.Name}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var deleteResult = await _productService.DeleteAsync(SelectedProduct.ProductId);

                if (deleteResult.Success)
                {
                    ShowStatusMessage("Produto excluído com sucesso!", false);
                    await LoadProductsAsync();
                }
                else
                {
                    var errorMsg = deleteResult.Errors?.FirstOrDefault() ?? deleteResult.Message ?? "Erro ao excluir produto";
                    ShowStatusMessage(errorMsg, true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao excluir produto: {ex.Message}", true);
            }
        }

        private void CancelProductForm()
        {
            IsProductFormOpen = false;
            ClearProductForm();
        }

        private void ClearProductForm()
        {
            ProductId = 0;
            ProductCode = null;
            ProductName = string.Empty;
            ProductDescription = null;
            ProductShortDescription = null;
            ProductBarcode = null;
            ProductSKU = null;
            ProductWeight = null;
            ProductDimensions = null;
            ProductCategoryId = 0;
            ProductSupplierId = 0;
            ProductSalePrice = 0;
            ProductCostPrice = 0;
            ProductDiscountPercentage = null;
            ProductTaxRate = null;
            ProductPhotoUrl = null;
            ProductStatus = ProductStatus.Active;
            ProductIsFeatured = false;
            ProductAllowBackorder = false;
            ProductDisplayOrder = 0;
            ProductControlsStock = true;
            ProductMinimumStock = null;
            ProductMaximumStock = null;
            ProductReorderPoint = null;
            ProductHasExpirationDate = false;
            ProductExpirationDays = null;
            ProductExpirationWarningDays = null;
            ProfitMargin = 0;
            FinalPrice = 0;
        }

        private void UploadProductPhoto()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Selecionar Foto do Produto"
            };

            if (dialog.ShowDialog() == true)
            {
                var sourceFilePath = dialog.FileName;

                if (!_fileStorageService.IsValidImage(sourceFilePath))
                {
                    ShowStatusMessage("O arquivo selecionado não é uma imagem válida.", true);
                    return;
                }

                Task.Run(async () =>
                {
                    try
                    {
                        var savedPath = await _fileStorageService.SaveLogoAsync(sourceFilePath);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProductPhotoUrl = savedPath;
                            ShowStatusMessage("Foto carregada com sucesso!", false);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowStatusMessage($"Erro ao fazer upload da foto: {ex.Message}", true);
                        });
                    }
                });
            }
        }

        private void RemoveProductPhoto()
        {
            ProductPhotoUrl = null;
        }

        private async Task ExportProductsAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Arquivo CSV|*.csv|Arquivo Excel|*.xlsx",
                    Title = "Exportar Produtos",
                    FileName = $"Produtos_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() != true) return;

                IsLoading = true;

                var filePath = dialog.FileName;
                var extension = System.IO.Path.GetExtension(filePath).ToLower();

                if (extension == ".csv")
                {
                    await ExportToCsvAsync(filePath);
                }
                else if (extension == ".xlsx")
                {
                    await ExportToExcelAsync(filePath);
                }

                ShowStatusMessage($"Dados exportados com sucesso para: {filePath}", false);
                
                // Abrir o arquivo após exportação
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao exportar dados: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToCsvAsync(string filePath)
        {
            await Task.Run(() =>
            {
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                
                // Cabeçalho
                writer.WriteLine("Código,Nome,Categoria,SKU,Código de Barras,Preço Custo,Preço Venda,Estoque Atual,Status");

                // Dados
                foreach (var product in FilteredProducts)
                {
                    var line = $"\"{product.Code}\",\"{product.Name}\",\"{product.CategoryName}\"," +
                              $"\"{product.SKU}\",\"{product.Barcode}\"," +
                              $"{product.CostPrice:F2},{product.SalePrice:F2}," +
                              $"{product.CurrentStock},\"{product.Status}\"";
                    writer.WriteLine(line);
                }
            });
        }

        private async Task ExportToExcelAsync(string filePath)
        {
            await Task.Run(() =>
            {
                // Simulação básica de exportação Excel usando CSV
                // Para uma solução completa, use a biblioteca EPPlus ou ClosedXML
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                
                // Cabeçalho
                writer.WriteLine("Código\tNome\tCategoria\tSKU\tCódigo de Barras\tPreço Custo\tPreço Venda\tEstoque Atual\tStatus");

                // Dados
                foreach (var product in FilteredProducts)
                {
                    var line = $"{product.Code}\t{product.Name}\t{product.CategoryName}\t" +
                              $"{product.SKU}\t{product.Barcode}\t" +
                              $"{product.CostPrice:F2}\t{product.SalePrice:F2}\t" +
                              $"{product.CurrentStock}\t{product.Status}";
                    writer.WriteLine(line);
                }
            });
        }

        #endregion

        #region Category Methods

        private void OpenCategoryForm()
        {
            ClearCategoryForm();
            IsCategoryFormOpen = true;
        }

        private void EditCategory()
        {
            if (SelectedCategory == null) return;

            CategoryId = SelectedCategory.CategoryId;
            CategoryName = SelectedCategory.Name;
            CategoryDescription = SelectedCategory.Description;
            CategoryCode = SelectedCategory.Code;
            CategoryIsActive = SelectedCategory.IsActive;

            IsCategoryFormOpen = true;
        }

        private async Task SaveCategoryAsync()
        {
            if (!CanSaveCategory()) return;

            IsSaving = true;

            try
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = CategoryId,
                    Name = CategoryName,
                    Description = CategoryDescription ?? string.Empty,
                    Code = CategoryCode ?? string.Empty,
                    IsActive = CategoryIsActive
                };

                var result = CategoryId == 0
                    ? await _categoryService.AddAsync(categoryDto)
                    : await _categoryService.UpdateAsync(categoryDto);

                if (result.Success)
                {
                    ShowStatusMessage(CategoryId == 0 ? "Categoria criada com sucesso!" : "Categoria atualizada com sucesso!", false);
                    IsCategoryFormOpen = false;
                    await LoadCategoriesAsync();
                }
                else
                {
                    var errors = "";
                    if (result.Errors?.Any() == true)
                         errors += "\n• " + string.Join("\n• ", result.Errors);
                    ShowStatusMessage(errors ?? "Erro ao salvar categoria", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao salvar categoria: {ex.Message}", true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            var hasProducts = await _categoryService.HasProductsAsync(SelectedCategory.CategoryId);

            if (hasProducts)
            {
                ShowStatusMessage("Não é possível excluir uma categoria que possui produtos vinculados.", true);
                return;
            }

            var result = MessageBox.Show(
                $"Deseja realmente excluir a categoria '{SelectedCategory.Name}'?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var deleteResult = await _categoryService.DeleteAsync(SelectedCategory.CategoryId);

                if (deleteResult)
                {
                    ShowStatusMessage("Categoria excluída com sucesso!", false);
                    await LoadCategoriesAsync();
                }
                else
                {
                    ShowStatusMessage("Erro ao excluir categoria", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao excluir categoria: {ex.Message}", true);
            }
        }

        private void CancelCategoryForm()
        {
            IsCategoryFormOpen = false;
            ClearCategoryForm();
        }

        private void ClearCategoryForm()
        {
            CategoryId = 0;
            CategoryName = string.Empty;
            CategoryDescription = null;
            CategoryCode = null;
            CategoryIsActive = true;
        }

        #endregion

        #region Expiration Methods

        private void OpenExpirationForm()
        {
            if (SelectedProduct == null) return;

            ClearExpirationForm();
            ExpirationProductId = SelectedProduct.ProductId;
            IsExpirationFormOpen = true;
        }

        private void EditExpiration()
        {
            if (SelectedExpiration == null) return;

            ExpirationId = SelectedExpiration.ExpirationId;
            ExpirationProductId = SelectedExpiration.ProductId;
            ExpirationDate = SelectedExpiration.ExpirationDate;
            ExpirationQuantity = SelectedExpiration.Quantity;
            BatchNumber = SelectedExpiration.BatchNumber;
            ExpirationNotes = SelectedExpiration.Notes;

            IsExpirationFormOpen = true;
        }

        private async Task SaveExpirationAsync()
        {
            if (!CanSaveExpiration()) return;

            IsSaving = true;

            try
            {
                var expirationDto = new ExpirationDto
                {
                    ExpirationId = ExpirationId,
                    ProductId = ExpirationProductId,
                    ExpirationDate = ExpirationDate,
                    Quantity = ExpirationQuantity,
                    BatchNumber = BatchNumber ?? string.Empty,
                    Notes = ExpirationNotes ?? string.Empty
                };

                var result = ExpirationId == 0
                    ? await _expirationService.AddAsync(expirationDto)
                    : await _expirationService.UpdateAsync(expirationDto);

                if (result.Success)
                {
                    ShowStatusMessage(ExpirationId == 0 ? "Validade adicionada com sucesso!" : "Validade atualizada com sucesso!", false);
                    IsExpirationFormOpen = false;
                    await LoadExpirationsAsync();
                }
                else
                {
                    var errors = "";
                    if (result.Errors?.Any() == true)
                        errors += "\n• " + string.Join("\n• ", result.Errors);
                    ShowStatusMessage(errors ?? "Erro ao salvar Validade", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao salvar validade: {ex.Message}", true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task DeleteExpirationAsync()
        {
            if (SelectedExpiration == null) return;

            var result = MessageBox.Show(
                "Deseja realmente excluir este registro de validade?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var deleteResult = await _expirationService.DeleteAsync(SelectedExpiration.ExpirationId);

                if (deleteResult)
                {
                    ShowStatusMessage("Validade excluída com sucesso!", false);
                    await LoadExpirationsAsync();
                }
                else
                {
                    ShowStatusMessage("Erro ao excluir validade", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao excluir validade: {ex.Message}", true);
            }
        }

        private void CancelExpirationForm()
        {
            IsExpirationFormOpen = false;
            ClearExpirationForm();
        }

        private void ClearExpirationForm()
        {
            ExpirationId = 0;
            ExpirationProductId = 0;
            ExpirationDate = DateTime.Now.AddMonths(6);
            ExpirationQuantity = 0;
            BatchNumber = null;
            ExpirationNotes = null;
        }

        #endregion

        #region Stock Movement Methods

        private void OpenMovementForm()
        {
            if (SelectedProduct == null) return;

            ClearMovementForm();
            MovementProductId = SelectedProduct.ProductId;
            IsMovementFormOpen = true;
        }

        private async Task SaveMovementAsync()
        {
            if (!CanSaveMovement()) return;

            IsSaving = true;

            try
            {
                var movementDto = new StockMovementDto
                {
                    ProductId = MovementProductId,
                    UserId = _currentUserContext.UserId ?? 0,
                    Quantity = MovementQuantity,
                    Type = MovementType,
                    Notes = MovementNotes ?? string.Empty,
                    UnitCost = MovementUnitCost,
                    TotalCost = MovementTotalCost,
                    Date = DateTime.UtcNow
                };

                var result = await _stockMovementService.AddAsync(movementDto);

                if (result.Success)
                {
                    ShowStatusMessage("Movimentação registrada com sucesso!", false);
                    IsMovementFormOpen = false;
                    await LoadStockMovementsAsync();
                    await LoadLowStockAsync();
                }
                else
                {
                    var errors = "";
                    if (result.Errors?.Any() == true)
                        errors += "\n• " + string.Join("\n• ", result.Errors);
                    ShowStatusMessage(errors ?? "Erro ao registrar movimentação", true);
                 
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Erro ao registrar movimentação: {ex.Message}", true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void CancelMovementForm()
        {
            IsMovementFormOpen = false;
            ClearMovementForm();
        }

        private void ClearMovementForm()
        {
            MovementProductId = 0;
            MovementQuantity = 0;
            MovementType = StockMovementType.Entry;
            MovementNotes = null;
            MovementUnitCost = null;
            MovementTotalCost = null;
        }

        #endregion

        #region Filters

        private void ApplyFilters()
        {
            var filtered = Products.AsEnumerable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.Barcode?.ToLower().Contains(search) ?? false) ||
                    (p.SKU?.ToLower().Contains(search) ?? false));
            }

            // Status filter
            if (FilterStatus.HasValue)
            {
                filtered = filtered.Where(p => p.Status == FilterStatus.Value);
            }

            // Category filter
            if (SelectedCategory != null)
            {
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.CategoryId);
            }

            // Low stock filter
            if (FilterLowStock)
            {
                filtered = filtered.Where(p => p.MinimumStock.HasValue && p.CurrentStock <= p.MinimumStock.Value);
            }

            // Featured filter
            if (FilterFeatured)
            {
                filtered = filtered.Where(p => p.IsFeatured);
            }

            // Calcular paginação
            var filteredList = filtered.ToList();
            TotalRecords = filteredList.Count;
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);

            // Ajustar página atual se necessário
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }

            // Aplicar paginação
            var paginatedItems = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            FilteredProducts.Clear();
            foreach (var product in paginatedItems)
            {
                FilteredProducts.Add(product);
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            FilterStatus = null;
            SelectedCategory = null;
            FilterLowStock = false;
            FilterFeatured = false;
            ApplyFilters();
        }

        #endregion

        #region Validations

        private void ValidateProductName()
        {
            IsProductNameValid = !string.IsNullOrWhiteSpace(ProductName) && ProductName.Length >= 3;
        }

        private void ValidatePrice()
        {
            IsPriceValid = ProductSalePrice >= 0 && ProductCostPrice >= 0;
        }

        private void ValidateCategoryName()
        {
            IsCategoryNameValid = !string.IsNullOrWhiteSpace(CategoryName) && CategoryName.Length >= 3;
        }

        private bool CanSaveProduct()
        {
            return IsProductNameValid && IsPriceValid && ProductCategoryId > 0 && !IsSaving;
        }

        private bool CanSaveCategory()
        {
            return IsCategoryNameValid && !IsSaving;
        }

        private bool CanSaveExpiration()
        {
            return ExpirationProductId > 0 && ExpirationQuantity > 0 && !IsSaving;
        }

        private bool CanSaveMovement()
        {
            return MovementProductId > 0 && MovementQuantity > 0 && !IsSaving;
        }

        #endregion

        #region Calculations

        private void CalculateProfitMargin()
        {
            if (ProductSalePrice > 0)
            {
                ProfitMargin = ((ProductSalePrice - ProductCostPrice) / ProductSalePrice) * 100;
            }
            else
            {
                ProfitMargin = 0;
            }
        }

        private void CalculateFinalPrice()
        {
            if (ProductDiscountPercentage.HasValue)
            {
                FinalPrice = ProductSalePrice - (ProductSalePrice * ProductDiscountPercentage.Value / 100);
            }
            else
            {
                FinalPrice = ProductSalePrice;
            }
        }

        private void CalculateMovementTotalCost()
        {
            if (MovementUnitCost.HasValue)
            {
                MovementTotalCost = MovementUnitCost.Value * MovementQuantity;
            }
        }

        private void UpdateStatistics()
        {
            TotalProducts = Products.Count;
            ActiveProducts = Products.Count(p => p.Status == ProductStatus.Active);
            LowStockCount = LowStockProducts.Count;
            // Calcular ExpiredCount manualmente já que IsExpired não existe no DTO
            ExpiredCount = Expirations.Count(e => e.ExpirationDate < DateTime.Now);
            NearExpirationCount = NearExpirations.Count;
        }

        #endregion

        #region Helpers

        private void ShowStatusMessage(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
            _statusMessageTimer.Stop();
            _statusMessageTimer.Start();
        }

        #endregion
    }
}
