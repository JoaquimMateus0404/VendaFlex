namespace VendaFlex.Core.DTOs
{
    public class CompanyConfigDto
    {
        public int CompanyConfigId { get; set; }
        public string CompanyName { get; set; }
        public string IndustryType { get; set; }
        public string TaxRegime { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
        public string Currency { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal DefaultTaxRate { get; set; }
        public string InvoiceFooterText { get; set; }
        public string InvoicePrefix { get; set; }
        public int NextInvoiceNumber { get; set; }
        public int InvoiceFormat { get; set; }
        public bool IncludeCustomerData { get; set; }
        public bool AllowAnonymousInvoice { get; set; }
        public string BusinessHours { get; set; }
        public bool IsActive { get; set; }
    }
}
