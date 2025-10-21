﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class StockMovement : AuditableEntity
    {
        [Key]
        public int StockMovementId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int? PreviousQuantity { get; set; }

        public int? NewQuantity { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public StockMovementType Type { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string Reference { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }

    public enum StockMovementType
    {
        Entry = 1,
        Exit = 2,
        Adjustment = 3,
        Transfer = 4,
        Return = 5,
        Loss = 6
    }
}
