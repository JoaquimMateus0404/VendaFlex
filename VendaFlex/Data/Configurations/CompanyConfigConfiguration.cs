using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class CompanyConfigConfiguration : IEntityTypeConfiguration<CompanyConfig>
    {
        public void Configure(EntityTypeBuilder<CompanyConfig> builder)
        {
            builder.HasKey(c => c.CompanyConfigId);
        }
    }
}
