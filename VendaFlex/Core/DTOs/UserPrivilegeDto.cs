namespace VendaFlex.Core.DTOs
{
    public class UserPrivilegeDto
    {
        public int UserPrivilegeId { get; set; }
        public int UserId { get; set; }
        public int PrivilegeId { get; set; }
        public DateTime GrantedAt { get; set; }
        public int? GrantedByUserId { get; set; }
    }
}
