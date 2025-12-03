using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Sales
{
    public class PdvViewModel : BaseViewModel
    {
        private readonly ICompanyConfigService _companyConfigService;
        private readonly ISessionService _sessionService;
        private readonly IPersonService _personService;
        private readonly IProductService _productService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceProductService _invoiceProductService;
        private readonly IStockService _stockService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IExpirationService _expirationService;
        private readonly IReceiptPrintService _printService;
        private readonly ICategoryService _categoryService;

        private DispatcherTimer? _clockTimer;

        // Estado básico
        private string _currency = "AOA";
        private string _currencySymbol = "Kz";
        private decimal _taxRatePercent = 0m; // Ex.: 14 => 14%
        private bool _allowAnonymousInvoice = true;

        // Header (Relógio e Usuário)
        private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
        private string _userName = string.Empty;
        private string _userInitials = string.Empty;

        // Cliente
        private PersonDto? _selectedCustomer;
        private string _customerSearchTerm = string.Empty;
    private ObservableCollection<PersonDto> _customerSuggestions = new();

        // Produto / Busca rápida
        private string _productSearchTerm = string.Empty;
        private ProductDto? _selectedProductSuggestion;
        private ObservableCollection<ProductDto> _productSuggestions = new();

        // Catálogo (modal)
        private bool _isCatalogOpen;
        private string _catalogSearchTerm = string.Empty;
        private ObservableCollection<ProductDto> _catalogProducts = new();
        private ObservableCollection<CategoryDto> _catalogCategories = new();
        private CategoryDto? _selectedCatalogCategory;

        // Carrinho
        private ObservableCollection<CartItemViewModel> _cartItems = new();

        // Totais
        private decimal _subTotal;
        private decimal _discountTotal;
        private decimal _taxTotal;
        private decimal _grandTotal;

        // Pagamentos
        private ObservableCollection<PaymentEntry> _payments = new();
        private ObservableCollection<PaymentTypeDto> _paymentTypes = new();
        private PaymentTypeDto? _selectedPaymentType;
        private decimal _paymentAmount;
        
    // Feedback do Catálogo
    private string _catalogToastMessage = string.Empty;
    private bool _isCatalogToastVisible;

        // Estado UI
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public PdvViewModel(
            ICompanyConfigService companyConfigService,
            ISessionService sessionService,
            IPersonService personService,
            IProductService productService,
            IInvoiceService invoiceService,
            IInvoiceProductService invoiceProductService,
            IStockService stockService,
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IExpirationService expirationService,
            IReceiptPrintService printService,
            ICategoryService categoryService)
        {
            _companyConfigService = companyConfigService;
            _sessionService = sessionService;
            _personService = personService;
            _productService = productService;
            _invoiceService = invoiceService;
            _invoiceProductService = invoiceProductService;
            _stockService = stockService;
            _paymentService = paymentService;
            _paymentTypeService = paymentTypeService;
            _expirationService = expirationService;
            _printService = printService;
            _categoryService = categoryService;

            // Comandos
            InitializeCommand = new AsyncCommand(InitializeAsync);

            SearchProductCommand = new AsyncCommand(SearchProductsAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(ProductSearchTerm));
            AddProductByCodeCommand = new AsyncCommand(AddProductByCodeAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(ProductSearchTerm));
            AddSelectedSuggestionCommand = new RelayCommand(async _ =>
            {
                if (SelectedProductSuggestion != null)
                {
                    await AddProductToCartAsync(SelectedProductSuggestion, 1);
                }
            }, _ => SelectedProductSuggestion != null && !IsBusy);

            // Catálogo
            OpenCatalogCommand = new AsyncCommand(OpenCatalogAsync, () => !IsBusy);
            CloseCatalogCommand = new RelayCommand(_ => IsCatalogOpen = false);
            SelectCustomerFromSuggestionsCommand = new RelayCommand(async p =>
            {
                if (p is PersonDto person)
                {
                    SelectedCustomer = person;
                    CustomerSearchTerm = person.Name;
                    CustomerSuggestions.Clear();
                    await Task.CompletedTask;
                }
            });
            AddProductFromCatalogCommand = new RelayCommand(async p =>
            {
                if (p is ProductDto prod)
                {
                    await AddProductToCartAsync(prod, 1);
                    _ = ShowCatalogToastAsync($"{prod.Name} adicionado ao carrinho");
                }
            });

            IncreaseItemQtyCommand = new RelayCommand(async item =>
            {
                if (item is CartItemViewModel ci)
                {
                    var prod = await _productService.GetByIdAsync(ci.ProductId);
                    if (!prod.Success || prod.Data == null)
                    {
                        StatusMessage = "Produto não encontrado.";
                        return;
                    }

                    var canReserve = await EnsureSellableAndReserveAsync(prod.Data, 1);
                    if (!canReserve)
                    {
                        StatusMessage = "Quantidade indisponível para venda (estoque/validade).";
                        return;
                    }

                    ci.Quantity += 1;
                    RecalculateTotals();
                }
            });
            DecreaseItemQtyCommand = new RelayCommand(async item =>
            {
                if (item is CartItemViewModel ci)
                {
                    if (ci.Quantity > 1)
                    {
                        // liberar 1 do reservado
                        await SafeReleaseReservationAsync(ci.ProductId, 1);
                        ci.Quantity -= 1;
                        RecalculateTotals();
                    }
                }
            });
            RemoveItemCommand = new RelayCommand(async item =>
            {
                if (item is CartItemViewModel ci)
                {
                    await SafeReleaseReservationAsync(ci.ProductId, ci.Quantity);
                    CartItems.Remove(ci);
                    RecalculateTotals();
                }
            });
            ClearCartCommand = new RelayCommand(async _ =>
            {
                // liberar reservas de todos os itens
                foreach (var ci in CartItems.ToList())
                {
                    await SafeReleaseReservationAsync(ci.ProductId, ci.Quantity);
                }
                CartItems.Clear();
                Payments.Clear();
                PaymentAmount = 0;
                SelectedPaymentType = null;
                RecalculateTotals();
            }, _ => CartItems.Any());

            AddPaymentCommand = new RelayCommand(_ =>
            {
                if (SelectedPaymentType == null || PaymentAmount <= 0) return;

                Payments.Add(new PaymentEntry
                {
                    PaymentTypeId = SelectedPaymentType.PaymentTypeId,
                    PaymentTypeName = SelectedPaymentType.Name,
                    Amount = PaymentAmount
                });
                PaymentAmount = 0;
                SelectedPaymentType = null;
                OnPropertyChanged(nameof(AmountPaid));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(ChangeDue));
                // garantir reavaliação do botão Finalizar
                ((AsyncCommand)FinalizeSaleCommand!).RaiseCanExecuteChanged();
            }, _ => SelectedPaymentType != null && PaymentAmount > 0);

            RemovePaymentCommand = new RelayCommand(entry =>
            {
                if (entry is PaymentEntry pe)
                {
                    Payments.Remove(pe);
                    OnPropertyChanged(nameof(AmountPaid));
                    OnPropertyChanged(nameof(RemainingAmount));
                    OnPropertyChanged(nameof(ChangeDue));
                    // garantir reavaliação do botão Finalizar
                    ((AsyncCommand)FinalizeSaleCommand!).RaiseCanExecuteChanged();
                }
            });

            FinalizeSaleCommand = new AsyncCommand(FinalizeSaleAsync, CanFinalizeSale);

            SelectAnonymousCustomerCommand = new RelayCommand(async _ => await SelectAnonymousCustomerAsync());

            // Inicialização imediata
            _ = InitializeAsync();

            // Monitorar alterações no carrinho para recalcular totais quando Quantity/Discount mudarem
            CartItems.CollectionChanged += CartItems_CollectionChanged;
        }

        #region Propriedades públicas
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (Set(ref _isBusy, value))
                {
                    ((AsyncCommand)SearchProductCommand).RaiseCanExecuteChanged();
                    ((AsyncCommand)AddProductByCodeCommand).RaiseCanExecuteChanged();
                    ((AsyncCommand)FinalizeSaleCommand).RaiseCanExecuteChanged();
                    ((AsyncCommand)OpenCatalogCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        // Header
        public string CurrentTime
        {
            get => _currentTime;
            set => Set(ref _currentTime, value);
        }
        public string UserName
        {
            get => _userName;
            set
            {
                if (Set(ref _userName, value))
                {
                    UserInitials = GetUserInitials(value);
                }
            }
        }
        public string UserInitials
        {
            get => _userInitials;
            set => Set(ref _userInitials, value);
        }

        // Config
        public string Currency
        {
            get => _currency;
            set => Set(ref _currency, value);
        }
        public string CurrencySymbol
        {
            get => _currencySymbol;
            set => Set(ref _currencySymbol, value);
        }
        public decimal TaxRatePercent
        {
            get => _taxRatePercent;
            set
            {
                if (Set(ref _taxRatePercent, value))
                {
                    RecalculateTotals();
                }
            }
        }

        // Cliente
        public PersonDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (Set(ref _selectedCustomer, value))
                {
                    ((AsyncCommand)FinalizeSaleCommand).RaiseCanExecuteChanged();
                }
            }
        }
        public string CustomerSearchTerm
        {
            get => _customerSearchTerm;
            set
            {
                if (Set(ref _customerSearchTerm, value))
                {
                    _ = SearchCustomersAsync();
                }
            }
        }
        public ObservableCollection<PersonDto> CustomerSuggestions
        {
            get => _customerSuggestions;
            set => Set(ref _customerSuggestions, value);
        }

        // Produto
        public string ProductSearchTerm
        {
            get => _productSearchTerm;
            set
            {
                if (Set(ref _productSearchTerm, value))
                {
                    ((AsyncCommand)SearchProductCommand).RaiseCanExecuteChanged();
                    ((AsyncCommand)AddProductByCodeCommand).RaiseCanExecuteChanged();
                }
            }
        }
        public ObservableCollection<ProductDto> ProductSuggestions
        {
            get => _productSuggestions;
            set => Set(ref _productSuggestions, value);
        }
        public ProductDto? SelectedProductSuggestion
        {
            get => _selectedProductSuggestion;
            set => Set(ref _selectedProductSuggestion, value);
        }

        // Catálogo (modal)
        public bool IsCatalogOpen
        {
            get => _isCatalogOpen;
            set => Set(ref _isCatalogOpen, value);
        }
        public string CatalogSearchTerm
        {
            get => _catalogSearchTerm;
            set
            {
                if (Set(ref _catalogSearchTerm, value))
                {
                    _ = SearchCatalogAsync();
                }
            }
        }
        public ObservableCollection<ProductDto> CatalogProducts
        {
            get => _catalogProducts;
            set => Set(ref _catalogProducts, value);
        }
        public ObservableCollection<CategoryDto> CatalogCategories
        {
            get => _catalogCategories;
            set => Set(ref _catalogCategories, value);
        }
        public CategoryDto? SelectedCatalogCategory
        {
            get => _selectedCatalogCategory;
            set
            {
                if (Set(ref _selectedCatalogCategory, value))
                {
                    _ = SearchCatalogAsync();
                }
            }
        }

        // Carrinho
        public ObservableCollection<CartItemViewModel> CartItems
        {
            get => _cartItems;
            set
            {
                if (Set(ref _cartItems, value))
                {
                    // Reanexar handlers
                    CartItems.CollectionChanged += CartItems_CollectionChanged;
                    foreach (var item in CartItems)
                    {
                        item.PropertyChanged -= CartItem_PropertyChanged;
                        item.PropertyChanged += CartItem_PropertyChanged;
                    }
                    RecalculateTotals();
                }
            }
        }

        // Totais
        public decimal SubTotal
        {
            get => _subTotal;
            set => Set(ref _subTotal, value);
        }
        public decimal DiscountTotal
        {
            get => _discountTotal;
            set => Set(ref _discountTotal, value);
        }
        public decimal TaxTotal
        {
            get => _taxTotal;
            set => Set(ref _taxTotal, value);
        }
        public decimal GrandTotal
        {
            get => _grandTotal;
            set => Set(ref _grandTotal, value);
        }

        // Pagamentos
        public ObservableCollection<PaymentEntry> Payments
        {
            get => _payments;
            set => Set(ref _payments, value);
        }
        public ObservableCollection<PaymentTypeDto> PaymentTypes
        {
            get => _paymentTypes;
            set => Set(ref _paymentTypes, value);
        }
        public PaymentTypeDto? SelectedPaymentType
        {
            get => _selectedPaymentType;
            set => Set(ref _selectedPaymentType, value);
        }
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set => Set(ref _paymentAmount, value);
        }

        // Feedback do Catálogo (toast)
        public string CatalogToastMessage
        {
            get => _catalogToastMessage;
            set => Set(ref _catalogToastMessage, value);
        }
        public bool IsCatalogToastVisible
        {
            get => _isCatalogToastVisible;
            set => Set(ref _isCatalogToastVisible, value);
        }

        public decimal AmountPaid => Payments.Sum(p => p.Amount);
        public decimal RemainingAmount => Math.Max(GrandTotal - AmountPaid, 0);
        public decimal ChangeDue => AmountPaid > GrandTotal ? AmountPaid - GrandTotal : 0;
        #endregion

        #region Comandos
        public ICommand InitializeCommand { get; }
        public ICommand SearchProductCommand { get; }
        public ICommand AddProductByCodeCommand { get; }
        public ICommand AddSelectedSuggestionCommand { get; }

    public ICommand OpenCatalogCommand { get; }
        public ICommand CloseCatalogCommand { get; }
        public ICommand AddProductFromCatalogCommand { get; }
    public ICommand SelectCustomerFromSuggestionsCommand { get; }

        public ICommand IncreaseItemQtyCommand { get; }
        public ICommand DecreaseItemQtyCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }

        public ICommand AddPaymentCommand { get; }
        public ICommand RemovePaymentCommand { get; }

        public ICommand FinalizeSaleCommand { get; }
        public ICommand SelectAnonymousCustomerCommand { get; }
        #endregion

        #region Inicialização
        private async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Carregando configurações do PDV...";

                var cfg = await _companyConfigService.GetAsync();
                if (cfg.Success && cfg.Data != null)
                {
                    Currency = cfg.Data.Currency ?? Currency;
                    CurrencySymbol = cfg.Data.CurrencySymbol ?? CurrencySymbol;
                    TaxRatePercent = cfg.Data.DefaultTaxRate;
                    _allowAnonymousInvoice = cfg.Data.AllowAnonymousInvoice;
                }

                var types = await _paymentTypeService.GetActiveAsync();
                if (types.Success && types.Data != null)
                {
                    PaymentTypes = new ObservableCollection<PaymentTypeDto>(types.Data);
                }

                await LoadCategoriesAsync();

                if (_allowAnonymousInvoice)
                {
                    await SelectAnonymousCustomerAsync();
                }

                InitializeClock();
                InitializeUserInfo();

                StatusMessage = "Pronto";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao inicializar PDV: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void InitializeClock()
        {
            // Atualiza imediatamente e inicia timer
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => { CurrentTime = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();
        }

        private void InitializeUserInfo()
        {
            try
            {
                var name = _sessionService?.CurrentUser?.Username ?? "Admin User";
                UserName = string.IsNullOrWhiteSpace(name) ? "Admin User" : name;
            }
            catch
            {
                UserName = "Admin User";
            }
        }

        private async Task ShowCatalogToastAsync(string message)
        {
            CatalogToastMessage = message;
            IsCatalogToastVisible = true;
            try
            {
                await Task.Delay(1800);
            }
            catch { /* ignore */ }
            finally
            {
                IsCatalogToastVisible = false;
            }
        }

        private static string GetUserInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "??";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return ($"{parts[0][0]}{parts[^1][0]}").ToUpper();
        }
        #endregion

        #region Catálogo de produtos (modal)
        private async Task OpenCatalogAsync()
        {
            IsCatalogOpen = true;
            await LoadCatalogAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var catResult = await _categoryService.GetActiveAsync();
                var list = new List<CategoryDto>();
                // Item "Todas Categorias"
                list.Add(new CategoryDto { CategoryId = 0, Name = "Todas Categorias", IsActive = true });

                if (catResult.Success && catResult.Data != null)
                {
                    list.AddRange(catResult.Data);
                }
                CatalogCategories = new ObservableCollection<CategoryDto>(list);
                SelectedCatalogCategory = CatalogCategories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar categorias: {ex.Message}";
            }
        }

        private async Task LoadCatalogAsync()
        {
            try
            {
                IEnumerable<ProductDto> products = Enumerable.Empty<ProductDto>();

                if (SelectedCatalogCategory != null && SelectedCatalogCategory.CategoryId > 0)
                {
                    var byCat = await _productService.GetByCategoryIdAsync(SelectedCatalogCategory.CategoryId);
                    if (byCat.Success && byCat.Data != null)
                        products = byCat.Data;
                }
                else
                {
                    var productsResult = await _productService.GetAllAsync();
                    if (productsResult.Success && productsResult.Data != null)
                        products = productsResult.Data;
                }

                // Enriquecer com estoque antes de publicar a coleção (evita necessidade de INotifyPropertyChanged no DTO)
                CatalogProducts = await EnrichWithStockAsync(products);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar catálogo: {ex.Message}";
            }
        }

        private async Task SearchCatalogAsync()
        {
            try
            {
                var term = CatalogSearchTerm;
                IEnumerable<ProductDto> products = Enumerable.Empty<ProductDto>();

                if (SelectedCatalogCategory != null && SelectedCatalogCategory.CategoryId > 0)
                {
                    var byCat = await _productService.GetByCategoryIdAsync(SelectedCatalogCategory.CategoryId);
                    if (byCat.Success && byCat.Data != null)
                        products = byCat.Data;
                }
                else
                {
                    var all = await _productService.GetAllAsync();
                    if (all.Success && all.Data != null)
                        products = all.Data;
                }

                if (!string.IsNullOrWhiteSpace(term))
                {
                    //(p.Code != null && p.Code.ToLowerInvariant().Contains(lower)) ||
                var lower = term.Trim().ToLowerInvariant();
                    products = products.Where(p =>
                        (p.Name != null && p.Name.ToLowerInvariant().Contains(lower)) ||
                        (p.SKU != null && p.SKU.ToLowerInvariant().Contains(lower)) ||
                        (p.Barcode != null && p.Barcode.ToLowerInvariant().Contains(lower))
                    );
                }

                CatalogProducts = await EnrichWithStockAsync(products);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro na busca: {ex.Message}";
            }
        }

        private async Task<ObservableCollection<ProductDto>> EnrichWithStockAsync(IEnumerable<ProductDto> products)
        {
            var list = new List<ProductDto>();
            foreach (var p in products)
            {
                if (p == null)
                    continue;

                if (p.ControlsStock)
                {
                    try
                    {
                        p.CurrentStock = await _stockService.GetAvailableQuantityAsync(p.ProductId);
                    }
                    catch
                    {
                        p.CurrentStock = 0;
                    }
                }
                else
                {
                    p.CurrentStock = 0;
                }
                list.Add(p);
            }
            return new ObservableCollection<ProductDto>(list);
        }
        #endregion

        #region Busca/Adição de produtos
        private async Task SearchProductsAsync()
        {
            try
            {
                IsBusy = true;
                ProductSuggestions.Clear();

                // Tenta buscas por diferentes chaves
                var term = ProductSearchTerm.Trim();
                var results = new List<ProductDto>();

                var byBarcode = await _productService.GetByBarcodeAsync(term);
                if (byBarcode.Success && byBarcode.Data != null)
                {
                    results.Add(byBarcode.Data);
                }

                var bySku = await _productService.GetBySKUAsync(term);
                if (bySku.Success && bySku.Data != null && results.All(p => p.ProductId != bySku.Data.ProductId))
                {
                    results.Add(bySku.Data);
                }

                var byCode = await _productService.GetByCodeAsync(term);
                if (byCode.Success && byCode.Data != null && results.All(p => p.ProductId != byCode.Data.ProductId))
                {
                    results.Add(byCode.Data);
                }

                if (!results.Any())
                {
                    var search = await _productService.SearchAsync(term);
                    if (search.Success && search.Data != null)
                    {
                        results.AddRange(search.Data);
                    }
                }

                ProductSuggestions = new ObservableCollection<ProductDto>(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro na busca de produtos: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddProductByCodeAsync()
        {
            try
            {
                IsBusy = true;
                var term = ProductSearchTerm.Trim();

                // Buscar por prioridade: código de barras -> SKU -> código -> busca
                ProductDto? product = null;
                var byBarcode = await _productService.GetByBarcodeAsync(term);
                if (byBarcode.Success) product = byBarcode.Data;

                if (product == null)
                {
                    var bySku = await _productService.GetBySKUAsync(term);
                    if (bySku.Success) product = bySku.Data;
                }
                if (product == null)
                {
                    var byCode = await _productService.GetByCodeAsync(term);
                    if (byCode.Success) product = byCode.Data;
                }
                if (product == null)
                {
                    var search = await _productService.SearchAsync(term);
                    if (search.Success) product = search.Data?.FirstOrDefault();
                }

                if (product != null)
                {
                    await AddProductToCartAsync(product, 1);
                    ProductSearchTerm = string.Empty;
                    ProductSuggestions.Clear();
                }
                else
                {
                    StatusMessage = "Produto não encontrado.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao adicionar produto: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> EnsureSellableAndReserveAsync(ProductDto product, int quantity)
        {
            if (quantity <= 0) return true;

            if (product.ControlsStock)
            {
                var available = await _stockService.GetAvailableQuantityAsync(product.ProductId);
                if (available <= 0) return false;

                // Considerar validade: não permitir uso de quantidade expirada
                if (product.HasExpirationDate)
                {
                    var expiredQty = await _expirationService.GetExpiredQuantityByProductAsync(product.ProductId);
                    var sellable = Math.Max(available - Math.Max(expiredQty, 0), 0);
                    if (quantity > sellable)
                        return false;
                }
                else
                {
                    if (quantity > available)
                        return false;
                }

                // Reservar a quantidade para evitar concorrência
                var reserved = await _stockService.ReserveQuantityAsync(product.ProductId, quantity);
                if (!reserved) return false;
            }

            return true;
        }

        private async Task SafeReleaseReservationAsync(int productId, int quantity)
        {
            try
            {
                if (quantity > 0)
                {
                    await _stockService.ReleaseReservedQuantityAsync(productId, quantity);
                }
            }
            catch
            {
                // logging poderia ser adicionado aqui
            }
        }

        private async Task AddProductToCartAsync(ProductDto product, int quantity)
        {
            // Verificar disponibilidade e reservar
            var ok = await EnsureSellableAndReserveAsync(product, quantity);
            if (!ok)
            {
                StatusMessage = "Estoque insuficiente ou produto expirado para a quantidade solicitada.";
                return;
            }

            // Merge se já existir no carrinho
            var existing = CartItems.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                CartItems.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductCode = string.IsNullOrWhiteSpace(product.Barcode) ? product.SKU : product.Barcode,
                    Name = product.Name,
                    UnitPrice = product.SalePrice,
                    Quantity = quantity,
                    Discount = 0
                });
            }

            RecalculateTotals();
        }
        #endregion

        #region Totais
        private void RecalculateTotals()
        {
            SubTotal = CartItems.Sum(i => Math.Max(i.UnitPrice, 0) * Math.Max(i.Quantity, 0));
            DiscountTotal = CartItems.Sum(i => Math.Max(i.Discount, 0));

            var taxable = Math.Max(SubTotal - DiscountTotal, 0);
            TaxTotal = Math.Round(taxable * (TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);

            GrandTotal = Math.Max(taxable + TaxTotal, 0);

            OnPropertyChanged(nameof(AmountPaid));
            OnPropertyChanged(nameof(RemainingAmount));
            OnPropertyChanged(nameof(ChangeDue));
            ((AsyncCommand)FinalizeSaleCommand).RaiseCanExecuteChanged();
        }
        #endregion

        #region Cliente
        private async Task SearchCustomersAsync()
        {
            try
            {
                var term = CustomerSearchTerm?.Trim();
                if (string.IsNullOrWhiteSpace(term))
                {
                    CustomerSuggestions.Clear();
                    return;
                }

                var result = await _personService.SearchAsync(term);
                IEnumerable<PersonDto> items = Enumerable.Empty<PersonDto>();
                if (result.Success && result.Data != null)
                {
                    items = result.Data.Where(p => p.IsActive && p.Type == PersonType.Customer);
                }

                CustomerSuggestions = new ObservableCollection<PersonDto>(items.Take(10));
            }
            catch
            {
                CustomerSuggestions.Clear();
            }
        }

        private async Task SelectAnonymousCustomerAsync()
        {
            const int anonymousId = 1;
            try
            {
                var res = await _personService.GetByIdAsync(anonymousId);
                if (res.Success && res.Data != null)
                {
                    SelectedCustomer = res.Data;
                }
                else
                {
                    SelectedCustomer = new PersonDto
                    {
                        PersonId = anonymousId,
                        Name = "Consumidor Final",
                        Type = PersonType.Customer,
                        IsActive = true
                    };
                }
                CustomerSearchTerm = string.Empty;
                CustomerSuggestions.Clear();
            }
            catch
            {
                SelectedCustomer = new PersonDto
                {
                    PersonId = anonymousId,
                    Name = "Consumidor Final",
                    Type = PersonType.Customer,
                    IsActive = true
                };
                CustomerSearchTerm = string.Empty;
                CustomerSuggestions.Clear();
            }
        }
        #endregion

        #region Finalização da venda
        private bool CanFinalizeSale()
        {
            return !IsBusy && CartItems.Any()  && AmountPaid >= GrandTotal && (SelectedCustomer != null || _allowAnonymousInvoice);
        }

        private async Task FinalizeSaleAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Emitindo fatura...";
                Debug.WriteLine("[FinalizeSaleAsync] Início da emissão de fatura");

                // Garantir usuário logado 
                var userId = _sessionService.CurrentUser?.UserId ?? 0;
                Debug.WriteLine($"[Login] UserId: {userId}");
                if (userId <= 0)
                {
                    StatusMessage = "Usuário não autenticado. Faça login para finalizar a venda.";
                    Debug.WriteLine("[Login] Falha: usuário não autenticado");
                    return;
                }

                // Revalidação de estoque/validade antes de concluir
                foreach (var ci in CartItems)
                {
                    Debug.WriteLine($"[Estoque] Validando produto ID {ci.ProductId}");
                    var prodResult = await _productService.GetByIdAsync(ci.ProductId);
                    if (!prodResult.Success || prodResult.Data == null)
                    {
                        StatusMessage = $"Produto ID {ci.ProductId} não encontrado.";
                        Debug.WriteLine($"[Estoque] Produto não encontrado: {ci.ProductId}");
                        return;
                    }

                    if (prodResult.Data.HasExpirationDate)
                    {
                        Debug.WriteLine($"[Validade] Produto {prodResult.Data.Name} possui validade");
                        var expiredQty = await _expirationService.GetExpiredQuantityByProductAsync(ci.ProductId);
                        var available = await _stockService.GetAvailableQuantityAsync(ci.ProductId);
                        var sellable = Math.Max(available - Math.Max(expiredQty, 0), 0);
                        Debug.WriteLine($"[Validade] Disponível: {available}, Expirado: {expiredQty}, Vendável: {sellable}");

                        if (sellable < ci.Quantity)
                        {
                            StatusMessage = $"Quantidade não vendável (validade) para o produto {prodResult.Data.Name}.";
                            Debug.WriteLine($"[Validade] Bloqueado: {prodResult.Data.Name}");
                            return;
                        }
                    }
                }

                // 1) Gerar número da fatura
                var numberResult = await _companyConfigService.GenerateInvoiceNumberAsync();
                var invoiceNumber = numberResult.Success && !string.IsNullOrWhiteSpace(numberResult.Data)
                    ? numberResult.Data
                    : $"INV-{DateTime.Now:yyyyMMddHHmmss}";
                Debug.WriteLine($"[Fatura] Número gerado: {invoiceNumber}");

                // 2) Montar DTO da fatura
                var invoice = new InvoiceDto
                {
                    InvoiceNumber = invoiceNumber,
                    Date = DateTime.Now,
                    DueDate = DateTime.Now,
                    PersonId = SelectedCustomer?.PersonId ?? 1,
                    UserId = userId,
                    Status = InvoiceStatus.Paid,
                    SubTotal = SubTotal,
                    DiscountAmount = DiscountTotal,
                    TaxAmount = TaxTotal,
                    Total = GrandTotal,
                    PaidAmount = AmountPaid,
                    ShippingCost = TaxTotal,
                    Notes = $"Venda finalizada no PDV em {DateTime.Now:dd/MM/yyyy HH:mm}",
                    InternalNotes = "Fatura gerada pelo sistema PDV VendaFlex",

                };
                Debug.WriteLine("[Fatura] DTO montado");

                // 3) Persistir fatura
                var savedInvoice = await _invoiceService.AddAsync(invoice);
                if (!savedInvoice.Success || savedInvoice.Data == null)
                {
                    StatusMessage = savedInvoice.Message ?? "Falha ao salvar fatura.";
                    if (savedInvoice.Errors?.Any() == true)
                        StatusMessage += "\n• " + string.Join("\n• ", savedInvoice.Errors);
                    Debug.WriteLine("[Fatura] Falha ao salvar: ", StatusMessage);
                    return;
                }

                var invoiceId = savedInvoice.Data.InvoiceId;
                Debug.WriteLine($"[Fatura] Salva com ID: {invoiceId}");

                // 4) Persistir itens
                foreach (var item in CartItems)
                {
                    var lineBase = (item.UnitPrice * item.Quantity);
                    var discountPercent = lineBase > 0 ? Math.Round((item.Discount / lineBase) * 100m, 4, MidpointRounding.AwayFromZero) : 0m;

                    var invItem = new InvoiceProductDto
                    {
                        InvoiceId = invoiceId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountPercentage = discountPercent,
                        TaxRate = TaxRatePercent // assumindo taxa fixa para todos os itens
                    };

                    Debug.WriteLine($"[Item] Tentando adicionar produto {item.ProductId} à fatura {invoiceId}");
                    var addItemResult = await _invoiceProductService.AddAsync(invItem);
                    
                    if (!addItemResult.Success)
                    {
                        StatusMessage = addItemResult.Message ?? "Falha ao salvar item de fatura.";
                        if (addItemResult.Errors?.Any() == true)
                            StatusMessage += "\n• " + string.Join("\n• ", addItemResult.Errors);
                        Debug.WriteLine($"[Item] Falha ao salvar produto {item.ProductId}: {StatusMessage}");
                        return;
                    }
                    
                    Debug.WriteLine($"[Item] Produto {item.ProductId} adicionado à fatura com sucesso");
                }

                // 5) Persistir pagamentos
                foreach (var p in Payments)
                {
                    var payment = new PaymentDto
                    {
                        InvoiceId = invoiceId,
                        PaymentTypeId = p.PaymentTypeId,
                        Amount = p.Amount,
                        PaymentDate = DateTime.Now,
                        Notes = "Pagamento",
                        Reference = invoiceNumber,
                        IsConfirmed = true // Pagamento confirmado no PDV
                    };

                    var addPayResult = await _paymentService.AddAsync(payment);
                    Debug.WriteLine($"[Pagamento] Tipo {p.PaymentTypeId}, Valor {p.Amount}");
                    if (!addPayResult.Success)
                    {
                        StatusMessage = addPayResult.Message ?? "Falha ao registrar pagamento.";
                        if (addPayResult.Errors?.Any() == true)
                            StatusMessage += "\n• " + string.Join("\n• ", addPayResult.Errors);
                        Debug.WriteLine("[Pagamento] Falha ao registrar: ", StatusMessage);
                        return;
                    }
                }

                // 6) Atualizar estoque
                foreach (var item in CartItems)
                {
                    var stockResult = await _stockService.GetByProductIdAsync(item.ProductId);
                    if (stockResult.Success && stockResult.Data != null)
                    {
                        var currentQty = stockResult.Data.Quantity;
                        var newQty = Math.Max(currentQty - item.Quantity, 0);
                        await _stockService.UpdateQuantityAsync(item.ProductId, newQty, userId);
                        Debug.WriteLine($"[Estoque] Produto {item.ProductId} atualizado: {currentQty} → {newQty}");

                        await SafeReleaseReservationAsync(item.ProductId, item.Quantity);
                    }
                    else
                    {
                        Debug.WriteLine($"[Estoque] Produto {item.ProductId} não encontrado para atualização");
                        await SafeReleaseReservationAsync(item.ProductId, item.Quantity);
                    }
                }

                // 7) Impressão
                var cfgResult = await _companyConfigService.GetAsync();
                if (cfgResult.Success && cfgResult.Data != null)
                {
                    var cfg = cfgResult.Data;
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoiceId);
                    var items = itemsResult.Success && itemsResult.Data != null ? itemsResult.Data : Enumerable.Empty<InvoiceProductDto>();
                    Debug.WriteLine($"[Impressão] Formato: {cfg.InvoiceFormat}");
                    await _printService.PrintAsync(cfg, savedInvoice.Data, items, cfg.InvoiceFormat.ToString());
                }

                StatusMessage = $"Fatura {invoiceNumber} emitida com sucesso.";
                Debug.WriteLine("[Finalização] Venda concluída com sucesso");

                // 8) Limpar carrinho
                CartItems.Clear();
                Payments.Clear();
                RecalculateTotals();
                OnPropertyChanged(nameof(AmountPaid));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(ChangeDue));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao finalizar venda: {ex.Message}";
                Debug.WriteLine($"[Erro] {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[FinalizeSaleAsync] Fim do processo");
            }
        }


        #endregion

        // Poderíamos ter um método para parar timer quando necessário
        public void Cleanup()
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer = null;
            }
        }

        // Handlers para recalcular totais ao editar Qtd/Desconto
        private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var obj in e.NewItems)
            {
                if (obj is CartItemViewModel ci)
                {
                    ci.PropertyChanged -= CartItem_PropertyChanged;
                    ci.PropertyChanged += CartItem_PropertyChanged;
                }
            }
        }
        if (e.OldItems != null)
        {
            foreach (var obj in e.OldItems)
            {
                if (obj is CartItemViewModel ci)
                {
                    ci.PropertyChanged -= CartItem_PropertyChanged;
                }
            }
        }
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var ci in CartItems)
            {
                ci.PropertyChanged -= CartItem_PropertyChanged;
                ci.PropertyChanged += CartItem_PropertyChanged;
            }
        }
        RecalculateTotals();
    }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartItemViewModel.Quantity) ||
            e.PropertyName == nameof(CartItemViewModel.Discount) ||
            e.PropertyName == nameof(CartItemViewModel.UnitPrice))
        {
            RecalculateTotals();
        }
    }
    }

    #region Tipos auxiliares
    public class CartItemViewModel : BaseViewModel
    {
        private int _productId;
        private string _productCode = string.Empty;
        private string _name = string.Empty;
        private decimal _unitPrice;
        private int _quantity = 1;
        private decimal _discount;

        public int ProductId
        {
            get => _productId;
            set => Set(ref _productId, value);
        }
        public string ProductCode
        {
            get => _productCode;
            set => Set(ref _productCode, value);
        }
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (Set(ref _unitPrice, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (Set(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }
        public decimal Discount
        {
            get => _discount;
            set
            {
                if (Set(ref _discount, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public decimal LineTotal => Math.Max((UnitPrice * Quantity) - Discount, 0);
    }

    public class PaymentEntry : BaseViewModel
    {
        private int _paymentTypeId;
        private string _paymentTypeName = string.Empty;
        private decimal _amount;

        public int PaymentTypeId
        {
            get => _paymentTypeId;
            set => Set(ref _paymentTypeId, value);
        }
        public string PaymentTypeName
        {
            get => _paymentTypeName;
            set => Set(ref _paymentTypeName, value);
        }
        public decimal Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }
    }
    #endregion
}
