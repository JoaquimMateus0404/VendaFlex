using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
    {
        public void Configure(EntityTypeBuilder<Privilege> builder)
        {
            builder.HasKey(p => p.PrivilegeId);
        }
    }
}
