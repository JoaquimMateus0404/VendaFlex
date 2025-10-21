using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    /// <summary>
    /// Classe base para entidades com código único
    /// </summary>
    public abstract class CodedEntity : AuditableEntity
    {
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string InternalCode { get; set; }

        [StringLength(100)]
        public string ExternalCode { get; set; }
    }

}
