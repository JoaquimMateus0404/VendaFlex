using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class UserPrivilegeConfiguration : IEntityTypeConfiguration<UserPrivilege>
    {
        public void Configure(EntityTypeBuilder<UserPrivilege> builder)
        {
            builder.HasKey(up => up.UserPrivilegeId);
            builder.HasOne(up => up.User)
                .WithMany(u => u.UserPrivileges)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(up => up.Privilege)
                .WithMany(p => p.UserPrivileges)
                .HasForeignKey(up => up.PrivilegeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Filtro compatível com User (tem filtro global)
            builder.HasQueryFilter(up => !up.User.IsDeleted);
        }
    }
}
