using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class UserPrivilege
    {
        [Key]
        public int UserPrivilegeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PrivilegeId { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public int? GrantedByUserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(PrivilegeId))]
        public virtual Privilege Privilege { get; set; }
    }
    
}
