using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region Expense Management

    #endregion


    public class CompanyConfig : AuditableEntity
    {
        [Key]
        public int CompanyConfigId { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(100)]
        public string? IndustryType { get; set; } // Tipo de negócio ou setor

        [StringLength(100)]
        public string? TaxRegime { get; set; } // Regime fiscal (ex: Simples, Geral)

        [StringLength(50)]
        public string? TaxId { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(200)]
        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(50)]
        public string? Currency { get; set; } = "AOA";

        /// Símbolo
        [StringLength(10)]
        public string? CurrencySymbol { get; set; } = "Kz";

        /// Taxa de imposto padrão aplicada às vendas (em porcentagem).
        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultTaxRate { get; set; } = 0;

        /// Texto do rodape da fatura.
        [StringLength(1000)]
        public string? InvoiceFooterText { get; set; }

        /// Prefixo usado nas faturas.
        [StringLength(100)]
        public string? InvoicePrefix { get; set; } = "INV";

        /// Próximo número de fatura a ser emitido.
        public int NextInvoiceNumber { get; set; } = 1;

        /// Define o formato padrão da fatura.
        [Required]
        public InvoiceFormatType InvoiceFormat { get; set; } = InvoiceFormatType.A4;

        /// Indica se os dados do cliente devem ser incluídos na fatura.
        public bool IncludeCustomerData { get; set; } = true;

        /// Permite emitir faturas sem cliente identificado (ex: vendas rápidas).
        public bool AllowAnonymousInvoice { get; set; } = false;


        [StringLength(100)]
        public string? BusinessHours { get; set; }

        public bool IsActive { get; set; } = true;



        /// <summary>
        /// Define os formatos disponíveis para impressão de faturas.
        /// </summary>
        public enum InvoiceFormatType
        {
            A4 = 0,     // Impressão em folha A4
            Rolo = 1    // Impressão em rolo (térmica)
        }

    }


}
