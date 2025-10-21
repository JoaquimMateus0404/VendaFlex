using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasIndex(al => al.Timestamp);
            builder.HasIndex(al => new { al.EntityName, al.EntityId });
            builder.HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Garantir filtro compatível com o filtro global de User
            builder.HasQueryFilter(al => !al.User.IsDeleted);
        }
    }
}
