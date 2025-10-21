using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VendaFlex.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.HasOne(e => e.ExpenseType)
                .WithMany(et => et.Expenses)
                .HasForeignKey(e => e.ExpenseTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
