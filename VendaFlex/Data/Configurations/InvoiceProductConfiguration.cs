using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class InvoiceProductConfiguration : IEntityTypeConfiguration<InvoiceProduct>
    {
        public void Configure(EntityTypeBuilder<InvoiceProduct> builder)
        {
            builder.HasOne(ip => ip.Invoice)
                .WithMany(i => i.InvoiceProducts)
                .HasForeignKey(ip => ip.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ip => ip.Product)
                .WithMany(p => p.InvoiceProducts)
                .HasForeignKey(ip => ip.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Filtros compatíveis com Invoice (tem filtro global) e Product (tem filtro global)
            builder.HasQueryFilter(ip => !ip.Invoice.IsDeleted && !ip.Product.IsDeleted);
        }
    }
}
