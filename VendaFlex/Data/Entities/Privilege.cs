using System.ComponentModel.DataAnnotations;

namespace VendaFlex.Data.Entities
{
    public class Privilege
    {
        [Key]
        public int PrivilegeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
    }

    
}
