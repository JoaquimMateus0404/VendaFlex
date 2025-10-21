using System.ComponentModel.DataAnnotations;

namespace VendaFlex.Data.Entities
{
    #region Base Classes

    #endregion

    public class ExpenseType
    {
        [Key]
        public int ExpenseTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

   
}
