using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class ExpenseTypeConfiguration : IEntityTypeConfiguration<ExpenseType>
    {
        public void Configure(EntityTypeBuilder<ExpenseType> builder)
        {
            builder.HasKey(et => et.ExpenseTypeId);
        }
    }
}
