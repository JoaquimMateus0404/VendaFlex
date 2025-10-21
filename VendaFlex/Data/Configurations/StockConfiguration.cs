using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(s => s.ProductId);

            // Filtro compatível com Product (tem filtro global)
            builder.HasQueryFilter(s => !s.Product.IsDeleted);
        }
    }
}
