using System.ComponentModel.DataAnnotations;

namespace VendaFlex.Data.Entities
{
    public class PaymentType
    {
        [Key]
        public int PaymentTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool RequiresReference { get; set; } = false;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    
}
