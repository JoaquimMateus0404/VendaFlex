using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.DTOs;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace VendaFlex.ViewModels.Sales
{
    /// <summary>
    /// ViewModel para gerenciamento completo de faturas, incluindo
    /// listagem, filtragem, pagamentos, ajustes e ações de fatura.
    /// </summary>
    public class InvoiceManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly ICompanyConfigService _companyConfigService;
        private readonly ISessionService _sessionService;
        private readonly IPersonService _personService;
        private readonly IReceiptPrintService _printService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceProductService _invoiceProductService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;

        #endregion

        #region Properties - Loading & Messages

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private SnackbarMessageQueue _messageQueue = new(TimeSpan.FromSeconds(3));
        public SnackbarMessageQueue MessageQueue
        {
            get => _messageQueue;
            set => Set(ref _messageQueue, value);
        }

        #endregion

        #region Properties - Statistics

        private int _totalPaidCount;
        public int TotalPaidCount
        {
            get => _totalPaidCount;
            set => Set(ref _totalPaidCount, value);
        }

        private int _totalPendingCount;
        public int TotalPendingCount
        {
            get => _totalPendingCount;
            set => Set(ref _totalPendingCount, value);
        }

        private int _totalCancelledCount;
        public int TotalCancelledCount
        {
            get => _totalCancelledCount;
            set => Set(ref _totalCancelledCount, value);
        }

        #endregion

        #region Properties - Filters

        private DateTime? _filterStartDate;
        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set => Set(ref _filterStartDate, value);
        }

        private DateTime? _filterEndDate;
        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set => Set(ref _filterEndDate, value);
        }

        private string _filterInvoiceNumber = string.Empty;
        public string FilterInvoiceNumber
        {
            get => _filterInvoiceNumber;
            set => Set(ref _filterInvoiceNumber, value);
        }

        private string _filterStatus = "Todos";
        public string FilterStatus
        {
            get => _filterStatus;
            set => Set(ref _filterStatus, value);
        }

        private string _filterCustomerName = string.Empty;
        public string FilterCustomerName
        {
            get => _filterCustomerName;
            set => Set(ref _filterCustomerName, value);
        }

        private string _filterCustomerNif = string.Empty;
        public string FilterCustomerNif
        {
            get => _filterCustomerNif;
            set => Set(ref _filterCustomerNif, value);
        }

        private string _filterOperatorName = string.Empty;
        public string FilterOperatorName
        {
            get => _filterOperatorName;
            set => Set(ref _filterOperatorName, value);
        }

        private PaymentTypeDto? _filterPaymentType;
        public PaymentTypeDto? FilterPaymentType
        {
            get => _filterPaymentType;
            set => Set(ref _filterPaymentType, value);
        }

        #endregion

        #region Properties - Pagination & Sorting

        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => Set(ref _pageNumber, value);
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (Set(ref _pageSize, value))
                {
                    _ = SearchAsync();
                }
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => Set(ref _totalPages, value);
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set => Set(ref _totalItems, value);
        }

        private string _sortColumn = "Data";
        public string SortColumn
        {
            get => _sortColumn;
            set
            {
                if (Set(ref _sortColumn, value))
                {
                    _ = SearchAsync();
                }
            }
        }

        private bool _sortAscending = false;
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (Set(ref _sortAscending, value))
                {
                    _ = SearchAsync();
                }
            }
        }

        #endregion

        #region Properties - Collections

        private ObservableCollection<InvoiceListItemDto> _invoices = new();
        public ObservableCollection<InvoiceListItemDto> Invoices
        {
            get => _invoices;
            set => Set(ref _invoices, value);
        }

        private ObservableCollection<PaymentTypeDto> _paymentTypes = new();
        public ObservableCollection<PaymentTypeDto> PaymentTypes
        {
            get => _paymentTypes;
            set => Set(ref _paymentTypes, value);
        }

        #endregion

        #region Properties - Selected Invoice

        private InvoiceListItemDto? _selectedInvoice;
        public InvoiceListItemDto? SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (Set(ref _selectedInvoice, value))
                {
                    _ = LoadInvoiceDetailsAsync();
                }
            }
        }

        private decimal _selectedInvoiceTotal;
        public decimal SelectedInvoiceTotal
        {
            get => _selectedInvoiceTotal;
            set => Set(ref _selectedInvoiceTotal, value);
        }

        private decimal _selectedInvoicePaid;
        public decimal SelectedInvoicePaid
        {
            get => _selectedInvoicePaid;
            set => Set(ref _selectedInvoicePaid, value);
        }

        private decimal _selectedInvoiceBalance;
        public decimal SelectedInvoiceBalance
        {
            get => _selectedInvoiceBalance;
            set => Set(ref _selectedInvoiceBalance, value);
        }

        private ObservableCollection<InvoiceProductDto> _selectedInvoiceItems = new();
        public ObservableCollection<InvoiceProductDto> SelectedInvoiceItems
        {
            get => _selectedInvoiceItems;
            set => Set(ref _selectedInvoiceItems, value);
        }

        private ObservableCollection<PaymentListItemDto> _selectedInvoicePayments = new();
        public ObservableCollection<PaymentListItemDto> SelectedInvoicePayments
        {
            get => _selectedInvoicePayments;
            set => Set(ref _selectedInvoicePayments, value);
        }

        private ObservableCollection<InvoiceHistoryItemDto> _selectedInvoiceHistory = new();
        public ObservableCollection<InvoiceHistoryItemDto> SelectedInvoiceHistory
        {
            get => _selectedInvoiceHistory;
            set => Set(ref _selectedInvoiceHistory, value);
        }

        private PaymentListItemDto? _selectedPayment;
        public PaymentListItemDto? SelectedPayment
        {
            get => _selectedPayment;
            set => Set(ref _selectedPayment, value);
        }

        #endregion

        #region Properties - New Payment

        private NewPaymentDto _newPayment = new();
        public NewPaymentDto NewPayment
        {
            get => _newPayment;
            set => Set(ref _newPayment, value);
        }

        #endregion

        #region Properties - Adjustments

        private decimal _adjustmentDiscount;
        public decimal AdjustmentDiscount
        {
            get => _adjustmentDiscount;
            set => Set(ref _adjustmentDiscount, value);
        }

        private string _adjustmentReason = string.Empty;
        public string AdjustmentReason
        {
            get => _adjustmentReason;
            set => Set(ref _adjustmentReason, value);
        }

        private decimal _adjustmentSurcharge;
        public decimal AdjustmentSurcharge
        {
            get => _adjustmentSurcharge;
            set => Set(ref _adjustmentSurcharge, value);
        }

        private string _surchargeReason = string.Empty;
        public string SurchargeReason
        {
            get => _surchargeReason;
            set => Set(ref _surchargeReason, value);
        }

        private PaymentListItemDto? _paymentToChange;
        public PaymentListItemDto? PaymentToChange
        {
            get => _paymentToChange;
            set => Set(ref _paymentToChange, value);
        }

        private PaymentTypeDto? _newPaymentType;
        public PaymentTypeDto? NewPaymentType
        {
            get => _newPaymentType;
            set => Set(ref _newPaymentType, value);
        }

        #endregion

        #region Properties - Stock Impact

        private ObservableCollection<StockImpactItemDto> _stockImpactItems = new();
        public ObservableCollection<StockImpactItemDto> StockImpactItems
        {
            get => _stockImpactItems;
            set => Set(ref _stockImpactItems, value);
        }

        #endregion

        #region Properties - Dialog

        private bool _isDetailsDialogOpen;
        public bool IsDetailsDialogOpen
        {
            get => _isDetailsDialogOpen;
            set => Set(ref _isDetailsDialogOpen, value);
        }

        #endregion

        #region Commands

        public ICommand ViewDetailsCommand { get; }
        public ICommand CloseDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand PrintInvoiceCommand { get; }
        public ICommand GeneratePdfCommand { get; }
        public ICommand DuplicateInvoiceCommand { get; }
        public ICommand ReopenInvoiceCommand { get; }
        public ICommand IssueCreditNoteCommand { get; }
        public ICommand IssueDebitNoteCommand { get; }
        public ICommand CancelInvoiceCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand RemovePaymentCommand { get; }
        public ICommand ApplyDiscountCommand { get; }
        public ICommand ApplySurchargeCommand { get; }
        public ICommand ChangePaymentTypeCommand { get; }

        #endregion

        #region Constructor

        public InvoiceManagementViewModel(
            ICompanyConfigService companyConfigService,
            ISessionService sessionService,
            IPersonService personService,
            IReceiptPrintService printService,
            IInvoiceService invoiceService,
            IInvoiceProductService invoiceProductService,
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IProductService productService,
            IStockService stockService)
        {
            _companyConfigService = companyConfigService ?? throw new ArgumentNullException(nameof(companyConfigService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _printService = printService ?? throw new ArgumentNullException(nameof(printService));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _invoiceProductService = invoiceProductService ?? throw new ArgumentNullException(nameof(invoiceProductService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));

            // Commands
            ViewDetailsCommand = new RelayCommand(async invoice => await ViewDetailsAsync(invoice as InvoiceListItemDto));
            CloseDetailsCommand = new RelayCommand(_ => CloseDetails());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            ExportCommand = new RelayCommand(async _ => await ExportAsync());
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            FirstPageCommand = new RelayCommand(async _ => await GoToFirstPageAsync(), _ => PageNumber > 1);
            PreviousPageCommand = new RelayCommand(async _ => await GoToPreviousPageAsync(), _ => PageNumber > 1);
            NextPageCommand = new RelayCommand(async _ => await GoToNextPageAsync(), _ => PageNumber < TotalPages);
            LastPageCommand = new RelayCommand(async _ => await GoToLastPageAsync(), _ => PageNumber < TotalPages);
            PrintInvoiceCommand = new RelayCommand(async _ => await PrintInvoiceAsync(), _ => SelectedInvoice != null);
            GeneratePdfCommand = new RelayCommand(
                async _ => await GeneratePdfAsync(), _ => SelectedInvoice != null);
            DuplicateInvoiceCommand = new RelayCommand(async _ => await DuplicateInvoiceAsync(), _ => SelectedInvoice != null);
            ReopenInvoiceCommand = new RelayCommand(async _ => await ReopenInvoiceAsync(), _ => SelectedInvoice != null);
            IssueCreditNoteCommand = new RelayCommand(async _ => await IssueCreditNoteAsync(), _ => SelectedInvoice != null);
            IssueDebitNoteCommand = new RelayCommand(async _ => await IssueDebitNoteAsync(), _ => SelectedInvoice != null);
            CancelInvoiceCommand = new RelayCommand(async _ => await CancelInvoiceAsync(), _ => SelectedInvoice != null);
            AddPaymentCommand = new RelayCommand(async _ => await AddPaymentAsync(), _ => SelectedInvoice != null && NewPayment?.PaymentTypeId > 0 && NewPayment.Amount > 0);
            RemovePaymentCommand = new RelayCommand(async payment => await RemovePaymentAsync(payment as PaymentListItemDto));
            ApplyDiscountCommand = new RelayCommand(async _ => await ApplyDiscountAsync(), _ => SelectedInvoice != null && AdjustmentDiscount > 0 && !string.IsNullOrWhiteSpace(AdjustmentReason));
            ApplySurchargeCommand = new RelayCommand(async _ => await ApplySurchargeAsync(), _ => SelectedInvoice != null && AdjustmentSurcharge > 0 && !string.IsNullOrWhiteSpace(SurchargeReason));
            ChangePaymentTypeCommand = new RelayCommand(async _ => await ChangePaymentTypeAsync(), _ => PaymentToChange != null && NewPaymentType != null);

            // Inicializar
            _ = InitializeAsync();
        }

        #endregion

        #region Initialization

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                // Carregar tipos de pagamento
                var paymentTypesResult = await _paymentTypeService.GetActiveAsync();
                if (paymentTypesResult.Success && paymentTypesResult.Data != null)
                {
                    PaymentTypes = new ObservableCollection<PaymentTypeDto>(paymentTypesResult.Data);
                }

                // Carregar faturas iniciais
                await SearchAsync();

                // Atualizar estatísticas
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao inicializar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Search & Filter Methods

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;

                // TODO: Implementar filtros e paginação no serviço
                var result = await _invoiceService.GetAllAsync();
                
                if (result.Success && result.Data != null)
                {
                    var invoices = result.Data;

                    // Aplicar filtros localmente (temporário)
                    if (FilterStartDate.HasValue)
                        invoices = invoices.Where(i => i.Date >= FilterStartDate.Value);
                    
                    if (FilterEndDate.HasValue)
                        invoices = invoices.Where(i => i.Date <= FilterEndDate.Value);
                    
                    if (!string.IsNullOrWhiteSpace(FilterInvoiceNumber))
                        invoices = invoices.Where(i => i.InvoiceNumber.Contains(FilterInvoiceNumber, StringComparison.OrdinalIgnoreCase));
                    
                    if (FilterStatus != "Todos")
                    {
                        var status = FilterStatus switch
                        {
                            "Paga" => InvoiceStatus.Paid,
                            "Pendente" => InvoiceStatus.Pending,
                            "Cancelada" => InvoiceStatus.Cancelled,
                            _ => (InvoiceStatus?)null
                        };
                        if (status.HasValue)
                            invoices = invoices.Where(i => i.Status == status.Value);
                    }

                    // Ordenação
                    invoices = SortColumn switch
                    {
                        "Data" => SortAscending ? invoices.OrderBy(i => i.Date) : invoices.OrderByDescending(i => i.Date),
                        "Número" => SortAscending ? invoices.OrderBy(i => i.InvoiceNumber) : invoices.OrderByDescending(i => i.InvoiceNumber),
                        "Total" => SortAscending ? invoices.OrderBy(i => i.Total) : invoices.OrderByDescending(i => i.Total),
                        _ => invoices.OrderByDescending(i => i.Date)
                    };

                    var invoicesList = invoices.ToList();
                    TotalItems = invoicesList.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    // Paginação local
                    var pagedInvoices = invoicesList.Skip((PageNumber - 1) * PageSize).Take(PageSize);

                    // Mapear para DTO de listagem
                    var listItems = pagedInvoices.Select(MapToListItem).ToList();
                    Invoices = new ObservableCollection<InvoiceListItemDto>(listItems);
                }
                else
                {
                    ShowMessage($"Erro ao buscar faturas: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao buscar faturas: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private InvoiceListItemDto MapToListItem(InvoiceDto invoice)
        {
            return new InvoiceListItemDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                Status = invoice.Status,
                Total = invoice.Total,
                Balance = invoice.Total - invoice.PaidAmount,
                CustomerName = "Cliente",  // TODO: Carregar do Person
                UserName = "Operador",     // TODO: Carregar do User
            };
        }

        private void ClearFilters()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            FilterInvoiceNumber = string.Empty;
            FilterStatus = "Todos";
            FilterCustomerName = string.Empty;
            FilterCustomerNif = string.Empty;
            FilterOperatorName = string.Empty;
            FilterPaymentType = null;
            PageNumber = 1;
            _ = SearchAsync();
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                var result = await _invoiceService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    var invoices = result.Data.ToList();
                    TotalPaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid);
                    TotalPendingCount = invoices.Count(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Confirmed);
                    TotalCancelledCount = invoices.Count(i => i.Status == InvoiceStatus.Cancelled);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar estatísticas: {ex.Message}");
            }
        }

        #endregion

        #region Pagination Methods

        private async Task GoToFirstPageAsync()
        {
            PageNumber = 1;
            await SearchAsync();
        }

        private async Task GoToPreviousPageAsync()
        {
            if (PageNumber > 1)
            {
                PageNumber--;
                await SearchAsync();
            }
        }

        private async Task GoToNextPageAsync()
        {
            if (PageNumber < TotalPages)
            {
                PageNumber++;
                await SearchAsync();
            }
        }

        private async Task GoToLastPageAsync()
        {
            PageNumber = TotalPages;
            await SearchAsync();
        }

        #endregion

        #region Invoice Details Methods

        private async Task ViewDetailsAsync(InvoiceListItemDto? invoice)
        {
            if (invoice == null) return;

            SelectedInvoice = invoice;
            await LoadInvoiceDetailsAsync();
            IsDetailsDialogOpen = true;
        }

        private void CloseDetails()
        {
            IsDetailsDialogOpen = false;
        }

        private async Task LoadInvoiceDetailsAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;

                // Carregar detalhes completos
                var invoiceResult = await _invoiceService.GetByIdAsync(SelectedInvoice.InvoiceId);
                if (invoiceResult.Success && invoiceResult.Data != null)
                {
                    var invoice = invoiceResult.Data;
                    SelectedInvoiceTotal = invoice.Total;
                    SelectedInvoicePaid = invoice.PaidAmount;
                    SelectedInvoiceBalance = invoice.Total - invoice.PaidAmount;

                    // TODO: Carregar produtos da fatura
                    SelectedInvoiceItems = new ObservableCollection<InvoiceProductDto>();

                    // Carregar pagamentos
                    await LoadInvoicePaymentsAsync(invoice.InvoiceId);

                    // TODO: Carregar histórico
                    SelectedInvoiceHistory = new ObservableCollection<InvoiceHistoryItemDto>();

                    // TODO: Carregar impacto no estoque
                    StockImpactItems = new ObservableCollection<StockImpactItemDto>();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao carregar detalhes: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadInvoicePaymentsAsync(int invoiceId)
        {
            try
            {
                // TODO: Implementar GetByInvoiceIdAsync no PaymentService
                SelectedInvoicePayments = new ObservableCollection<PaymentListItemDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar pagamentos: {ex.Message}");
            }
        }

        #endregion

        #region Invoice Actions

        private async Task PrintInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Imprimindo fatura...");
                
                // TODO: Implementar impressão
                await Task.Delay(1000);
                
                ShowMessage("Fatura impressa com sucesso!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao imprimir: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GeneratePdfAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Gerando PDF...");
                
                // TODO: Implementar geração de PDF
                await Task.Delay(1000);
                
                ShowMessage("PDF gerado com sucesso!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar PDF: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DuplicateInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja duplicar esta fatura?",
                "Duplicar Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Duplicando fatura...");
                
                // TODO: Implementar duplicação
                await Task.Delay(1000);
                
                ShowMessage("Fatura duplicada com sucesso!");
                await SearchAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao duplicar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReopenInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja reabrir esta fatura?",
                "Reabrir Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Reabrindo fatura...");
                
                // TODO: Implementar reabertura
                await Task.Delay(1000);
                
                ShowMessage("Fatura reaberta com sucesso!");
                await SearchAsync();
                await LoadInvoiceDetailsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao reabrir: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task IssueCreditNoteAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja emitir uma nota de crédito para esta fatura?",
                "Nota de Crédito",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Emitindo nota de crédito...");
                
                // TODO: Implementar nota de crédito
                await Task.Delay(1000);
                
                ShowMessage("Nota de crédito emitida com sucesso!");
                await SearchAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao emitir nota de crédito: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task IssueDebitNoteAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja emitir uma nota de débito para esta fatura?",
                "Nota de Débito",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Emitindo nota de débito...");
                
                // TODO: Implementar nota de débito
                await Task.Delay(1000);
                
                ShowMessage("Nota de débito emitida com sucesso!");
                await SearchAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao emitir nota de débito: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Tem certeza que deseja CANCELAR esta fatura? Esta ação não pode ser desfeita!",
                "Cancelar Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Cancelando fatura...");
                
                // TODO: Implementar cancelamento
                await Task.Delay(1000);
                
                ShowMessage("Fatura cancelada com sucesso!");
                await SearchAsync();
                await LoadInvoiceDetailsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao cancelar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Payment Methods

        private async Task AddPaymentAsync()
        {
            if (SelectedInvoice == null || NewPayment == null) return;

            if (NewPayment.PaymentTypeId <= 0)
            {
                ShowMessage("Selecione uma forma de pagamento!", true);
                return;
            }

            if (NewPayment.Amount <= 0)
            {
                ShowMessage("Informe um valor válido!", true);
                return;
            }

            if (NewPayment.Amount > SelectedInvoiceBalance)
            {
                ShowMessage($"Valor não pode exceder o saldo de Kz {SelectedInvoiceBalance:N2}!", true);
                return;
            }

            try
            {
                IsLoading = true;

                var paymentDto = new PaymentDto
                {
                    InvoiceId = SelectedInvoice.InvoiceId,
                    PaymentTypeId = NewPayment.PaymentTypeId,
                    Amount = NewPayment.Amount,
                    Reference = NewPayment.Reference ?? string.Empty,
                    PaymentDate = DateTime.UtcNow,
                    IsConfirmed = true
                };

                var result = await _paymentService.AddAsync(paymentDto);

                if (result.Success)
                {
                    ShowMessage("Pagamento registrado com sucesso!");
                    
                    // Limpar formulário
                    NewPayment = new NewPaymentDto();
                    
                    // Recarregar dados
                    await LoadInvoiceDetailsAsync();
                    await UpdateStatisticsAsync();
                }
                else
                {
                    ShowMessage($"Erro ao registrar pagamento: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao registrar pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemovePaymentAsync(PaymentListItemDto? payment)
        {
            if (payment == null) return;

            var result = MessageBox.Show(
                $"Deseja remover o pagamento de Kz {payment.Amount:N2}?",
                "Remover Pagamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;

                var deleteResult = await _paymentService.DeleteAsync(payment.PaymentId);

                if (deleteResult.Success)
                {
                    ShowMessage("Pagamento removido com sucesso!");
                    await LoadInvoiceDetailsAsync();
                    await UpdateStatisticsAsync();
                }
                else
                {
                    ShowMessage($"Erro ao remover pagamento: {deleteResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao remover pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Adjustment Methods

        private async Task ApplyDiscountAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Aplicando desconto...");
                
                // TODO: Implementar aplicação de desconto
                await Task.Delay(1000);
                
                ShowMessage("Desconto aplicado com sucesso!");
                AdjustmentDiscount = 0;
                AdjustmentReason = string.Empty;
                await LoadInvoiceDetailsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao aplicar desconto: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplySurchargeAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Aplicando acréscimo...");
                
                // TODO: Implementar aplicação de acréscimo
                await Task.Delay(1000);
                
                ShowMessage("Acréscimo aplicado com sucesso!");
                AdjustmentSurcharge = 0;
                SurchargeReason = string.Empty;
                await LoadInvoiceDetailsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao aplicar acréscimo: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ChangePaymentTypeAsync()
        {
            if (PaymentToChange == null || NewPaymentType == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Alterando forma de pagamento...");
                
                // TODO: Implementar alteração de forma de pagamento
                await Task.Delay(1000);
                
                ShowMessage("Forma de pagamento alterada com sucesso!");
                PaymentToChange = null;
                NewPaymentType = null;
                await LoadInvoiceDetailsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao alterar forma de pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Export & Refresh

        private async Task ExportAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = $"Faturas_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    ShowMessage("Exportando...");

                    var sb = new StringBuilder();
                    sb.AppendLine("Número;Data;Cliente;Operador;Status;Total;Pago;Saldo");

                    foreach (var invoice in Invoices)
                    {
                        sb.AppendLine($"{invoice.InvoiceNumber};{invoice.Date:dd/MM/yyyy HH:mm};{invoice.CustomerName};{invoice.UserName};{invoice.Status};{invoice.Total};{invoice.Total - invoice.Balance};{invoice.Balance}");
                    }

                    await File.WriteAllTextAsync(dialog.FileName, sb.ToString());
                    ShowMessage("Exportação concluída com sucesso!");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao exportar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            await SearchAsync();
            await UpdateStatisticsAsync();
            ShowMessage("Dados atualizados!");
        }

        #endregion

        #region Helper Methods

        private void ShowMessage(string message, bool isError = false)
        {
            MessageQueue?.Enqueue(message);
        }

        #endregion
    }

    #region Helper DTOs

    public class InvoiceListItemDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Total { get; set; }
        public decimal Balance { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNif { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerInitials { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string OperatorRole { get; set; } = string.Empty;
    }

    public class PaymentListItemDto
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    public class InvoiceHistoryItemDto
    {
        public string ActionDescription { get; set; } = string.Empty;
        public string ActionIcon { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class NewPaymentDto : BaseViewModel
    {
        private int _paymentTypeId;
        public int PaymentTypeId
        {
            get => _paymentTypeId;
            set => Set(ref _paymentTypeId, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }

        private string _reference = string.Empty;
        public string Reference
        {
            get => _reference;
            set => Set(ref _reference, value);
        }
    }

    public class StockImpactItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantitySold { get; set; }
        public decimal CurrentStock { get; set; }
    }

    #endregion
}
