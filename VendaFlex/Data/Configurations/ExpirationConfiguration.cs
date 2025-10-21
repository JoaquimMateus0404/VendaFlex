using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class ExpirationConfiguration : IEntityTypeConfiguration<Expiration>
    {
        public void Configure(EntityTypeBuilder<Expiration> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(e => e.Product)
                .WithMany(p => p.Expirations)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
