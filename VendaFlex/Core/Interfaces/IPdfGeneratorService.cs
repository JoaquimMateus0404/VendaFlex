using VendaFlex.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendaFlex.ViewModels.Reports;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para geração de documentos PDF
    /// </summary>
    public interface IPdfGeneratorService
    {
        #region Documentos Fiscais

        /// <summary>
        /// Gera um PDF da fatura e salva no caminho especificado
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="invoice">Dados da fatura</param>
        /// <param name="items">Itens da fatura</param>
        /// <param name="customer">Dados do cliente (opcional)</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateInvoicePdfAsync(
            CompanyConfigDto companyConfig,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            PersonDto? customer,
            string filePath);

        #endregion

        #region Relatórios de Vendas

        /// <summary>
        /// Gera relatório de vendas por período
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="salesData">Dados de vendas do período</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateSalesByPeriodReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<SalesByPeriodDto> salesData,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório dos produtos mais vendidos
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="topProducts">Lista dos produtos mais vendidos</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateTopProductsReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<TopProductDto> topProducts,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de vendas por cliente
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="salesByCustomer">Dados de vendas por cliente</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateSalesByCustomerReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<SalesByCustomerDto> salesByCustomer,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de margem de lucro
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="profitMargins">Dados de margem de lucro</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateProfitMarginReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<ProfitMarginDto> profitMargins,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de faturas por status
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="invoicesByStatus">Dados de faturas por status</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateInvoicesByStatusReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<InvoicesByStatusDto> invoicesByStatus,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        #endregion

        #region Relatórios Financeiros

        /// <summary>
        /// Gera relatório de fluxo de caixa
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="cashFlowData">Dados de fluxo de caixa</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateCashFlowReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<CashFlowDto> cashFlowData,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de formas de pagamento
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="paymentMethods">Dados de formas de pagamento</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GeneratePaymentMethodsReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<PaymentMethodDto> paymentMethods,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de contas a receber
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="accountsReceivable">Dados de contas a receber</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateAccountsReceivableReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<AccountsReceivableDto> accountsReceivable,
            string filePath);

        /// <summary>
        /// Gera demonstrativo financeiro completo
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="totalRevenue">Receita total</param>
        /// <param name="totalCost">Custo total</param>
        /// <param name="totalProfit">Lucro total</param>
        /// <param name="profitMargin">Margem de lucro</param>
        /// <param name="paymentMethods">Formas de pagamento</param>
        /// <param name="accountsReceivable">Contas a receber</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateFinancialStatementReportAsync(
            CompanyConfigDto companyConfig,
            decimal totalRevenue,
            decimal totalCost,
            decimal totalProfit,
            decimal profitMargin,
            IEnumerable<PaymentMethodDto> paymentMethods,
            IEnumerable<AccountsReceivableDto> accountsReceivable,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        #endregion

        #region Relatórios de Estoque

        /// <summary>
        /// Gera relatório de movimentação de estoque
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="stockMovements">Dados de movimentação de estoque</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateStockMovementsReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<StockMovementReportDto> stockMovements,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de produtos com estoque baixo
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="lowStockProducts">Lista de produtos com estoque baixo</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateLowStockReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<LowStockDto> lowStockProducts,
            string filePath);

        /// <summary>
        /// Gera relatório de produtos próximos ao vencimento
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="expiringProducts">Lista de produtos vencendo</param>
        /// <param name="daysThreshold">Dias de antecedência</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateExpiringProductsReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<ExpirationReportDto> expiringProducts,
            int daysThreshold,
            string filePath);

        #endregion

        #region Relatórios Gerenciais

        /// <summary>
        /// Gera dashboard executivo com KPIs principais
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="totalSales">Total de vendas</param>
        /// <param name="totalRevenue">Receita total</param>
        /// <param name="profitMargin">Margem de lucro</param>
        /// <param name="totalInvoices">Total de faturas</param>
        /// <param name="pendingAmount">Valor pendente</param>
        /// <param name="stockValue">Valor em estoque</param>
        /// <param name="topProducts">Top produtos</param>
        /// <param name="paymentMethods">Formas de pagamento</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateExecutiveDashboardReportAsync(
            CompanyConfigDto companyConfig,
            decimal totalSales,
            decimal totalRevenue,
            decimal profitMargin,
            int totalInvoices,
            decimal pendingAmount,
            decimal stockValue,
            IEnumerable<TopProductDto> topProducts,
            IEnumerable<PaymentMethodDto> paymentMethods,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório de desempenho por vendedor
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="salesByUser">Vendas por usuário</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateUserPerformanceReportAsync(
            CompanyConfigDto companyConfig,
            IEnumerable<SalesByUserDto> salesByUser,
            DateTime startDate,
            DateTime endDate,
            string filePath);

        /// <summary>
        /// Gera relatório geral mensal/anual consolidado - RELATÓRIO PRINCIPAL
        /// Este é o relatório mais completo com todas as análises do negócio
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="reportData">Dados consolidados do relatório</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateComprehensiveReportAsync(
            CompanyConfigDto companyConfig,
            ComprehensiveReportDto reportData,
            string filePath);

        #endregion
    }
}
