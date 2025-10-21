using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(p => p.Barcode);
            builder.HasIndex(p => p.SKU);
            builder.HasIndex(p => p.InternalCode).IsUnique();
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.SupplierId);
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(p => p.Stock)
                .WithOne(s => s.Product)
                .HasForeignKey<Stock>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(p => p.Supplier)
                .WithMany(pe => pe.SuppliedProducts)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
