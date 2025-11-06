using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class Expiration : AuditableEntity
    {
        [Key]
        public int ExpirationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(100)]
        public string BatchNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        /// <summary>
        /// Verifica se o lote já está vencido
        /// </summary>
        [NotMapped]
        public bool IsExpired => ExpirationDate.Date < DateTime.Now.Date;

        /// <summary>
        /// Calcula quantos dias faltam para vencer (negativo se já venceu)
        /// </summary>
        [NotMapped]
        public int DaysUntilExpiration => (ExpirationDate.Date - DateTime.Now.Date).Days;

        /// <summary>
        /// Verifica se está próximo do vencimento (usa dias de aviso do produto se disponível)
        /// Por padrão, considera próximo se faltar 30 dias ou menos
        /// </summary>
        [NotMapped]
        public bool IsNearExpiration
        {
            get
            {
                if (IsExpired) return false;
                
                // Se o produto tiver configuração de aviso, usa ela
                if (Product?.ExpirationWarningDays != null)
                {
                    return DaysUntilExpiration <= Product.ExpirationWarningDays.Value;
                }
                
                // Caso contrário, usa 30 dias como padrão
                return DaysUntilExpiration <= 30;
            }
        }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }

   
}
